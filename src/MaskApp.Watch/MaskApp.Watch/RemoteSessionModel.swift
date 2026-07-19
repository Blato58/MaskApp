import Combine
import Foundation
import WatchKit

struct RemoteFavorite: Codable, Identifiable, Equatable {
    let id: String
    let label: String
    let kind: String
    let colorHex: String
}

struct RemoteState: Codable, Equatable {
    let schemaVersion: Int
    let revision: Int64
    let generatedAt: String
    let staleAfterSeconds: Int
    let positionKind: String
    let positionTitle: String
    let positionText: String
    let currentCueId: String
    let currentCueLabel: String
    let nextCueLabel: String
    let maskConnected: Bool
    let maskConnectionText: String
    let readinessStatus: String
    let readinessSummary: String
    let watchReachable: Bool
    let companionStatus: String
    let foregroundExecutionRequired: Bool
    let phoneForeground: Bool
    let favorites: [RemoteFavorite]
}

private struct RemoteAction: Encodable {
    let kind: String
    let favoriteId: String
    let brightness: Int?
}

private struct RemoteEnvelope: Encodable {
    let schemaVersion: Int
    let messageId: UUID
    let senderInstanceId: String
    let sequence: Int64
    let sentAt: Date
    let action: RemoteAction
}

private struct RemoteProcessResult: Decodable {
    let schemaVersion: Int
    let succeeded: Bool
    let message: String
    let haptic: String
    let state: RemoteState
}

@MainActor
final class RemoteSessionModel: ObservableObject {
    static let protocolVersion = 1

    @Published private(set) var state: RemoteState?
    @Published private(set) var status = "Waiting for iPhone state..."
    @Published private(set) var phoneReachable = false
    @Published private(set) var busyActionIds: Set<String> = []
    @Published var brightness = 60.0

    private let transport: RemoteSessionTransport
    private let playHaptic: (WKHapticType) -> Void
    private let encoder: JSONEncoder
    private let decoder = JSONDecoder()
    private let senderInstanceId: String
    private var sequence: Int64
    private var brightnessTask: Task<Void, Never>?
    private var brightnessInFlight = false
    private var queuedBrightness: Int?

    init(
        transport: RemoteSessionTransport = WatchConnectivityTransport(),
        userDefaults: UserDefaults = .standard,
        playHaptic: @escaping (WKHapticType) -> Void = { WKInterfaceDevice.current().play($0) })
    {
        self.transport = transport
        self.playHaptic = playHaptic
        encoder = JSONEncoder()
        encoder.dateEncodingStrategy = .iso8601

        let senderKey = "watchRemoteSenderInstanceId"
        if let storedSender = userDefaults.string(forKey: senderKey), !storedSender.isEmpty {
            senderInstanceId = storedSender
        } else {
            let createdSender = UUID().uuidString
            senderInstanceId = createdSender
            userDefaults.set(createdSender, forKey: senderKey)
        }

        let sequenceKey = "watchRemoteSequence"
        sequence = max(Int64(userDefaults.integer(forKey: sequenceKey)), 0)
        self.userDefaults = userDefaults
        self.sequenceKey = sequenceKey

        phoneReachable = transport.isReachable
        applyApplicationContext(transport.receivedApplicationContext)

        transport.onApplicationContext = { [weak self] context in
            Task { @MainActor in self?.applyApplicationContext(context) }
        }
        transport.onReachabilityChanged = { [weak self] reachable in
            Task { @MainActor in self?.setReachability(reachable) }
        }
        transport.activate()
    }

    private let userDefaults: UserDefaults
    private let sequenceKey: String

    deinit {
        brightnessTask?.cancel()
    }

    var favorites: [RemoteFavorite] { state?.favorites ?? [] }

    var statusTitle: String {
        if !phoneReachable { return "iPhone unavailable" }
        if state?.maskConnected == true { return "Mask connected" }
        return state?.maskConnectionText ?? "Waiting for mask"
    }

    var statusSymbol: String {
        if !phoneReachable { return "iphone.slash" }
        return state?.maskConnected == true ? "checkmark.circle.fill" : "exclamationmark.circle.fill"
    }

    var statusColorName: String {
        if !phoneReachable { return "gray" }
        return state?.maskConnected == true ? "green" : "orange"
    }

    var positionTitle: String { state?.positionTitle ?? "No active position" }

    var positionText: String { state?.positionText ?? "Open Shining Mask on iPhone" }

    var currentCueLabel: String {
        guard let cue = state?.currentCueLabel, !cue.isEmpty else { return "Trigger current" }
        return cue
    }

    var nextCueLabel: String { state?.nextCueLabel ?? "" }

    func canSend(_ actionId: String) -> Bool {
        guard phoneReachable, !busyActionIds.contains(actionId) else { return false }
        if actionId == "Blackout" || actionId == "Stop" { return true }
        return state?.phoneForeground == true
    }

    func refreshCachedState() {
        applyApplicationContext(transport.receivedApplicationContext)
    }

    func sendAction(_ kind: String) {
        send(kind: kind, actionId: kind)
    }

    func sendFavorite(_ favorite: RemoteFavorite) {
        send(kind: "TriggerFavorite", favoriteId: favorite.id, actionId: "favorite:\(favorite.id)")
    }

    func restoreBrightness() {
        brightness = 60
        scheduleBrightnessSend()
    }

    func scheduleBrightnessSend() {
        brightnessTask?.cancel()
        let value = Int(brightness.rounded())
        brightnessTask = Task { [weak self] in
            try? await Task.sleep(for: .milliseconds(300))
            guard !Task.isCancelled else { return }
            self?.sendBrightness(value)
        }
    }

    private func setReachability(_ reachable: Bool) {
        phoneReachable = reachable
        if reachable {
            refreshCachedState()
        } else {
            status = "Open Shining Mask on iPhone. Commands are not queued."
        }
    }

    private func sendBrightness(_ value: Int) {
        guard canSend("SetBrightness") else {
            showUnavailableStatus()
            return
        }

        if brightnessInFlight {
            queuedBrightness = value
            return
        }

        brightnessInFlight = true
        send(
            kind: "SetBrightness",
            brightness: min(max(value, 1), 100),
            actionId: "SetBrightness") { [weak self] in
                guard let self else { return }
                self.brightnessInFlight = false
                if let queued = self.queuedBrightness {
                    self.queuedBrightness = nil
                    self.sendBrightness(queued)
                }
            }
    }

    private func send(
        kind: String,
        favoriteId: String = "",
        brightness: Int? = nil,
        actionId: String,
        completion: (() -> Void)? = nil)
    {
        guard canSend(actionId) else {
            showUnavailableStatus()
            completion?()
            return
        }

        sequence += 1
        userDefaults.set(sequence, forKey: sequenceKey)
        let envelope = RemoteEnvelope(
            schemaVersion: Self.protocolVersion,
            messageId: UUID(),
            senderInstanceId: senderInstanceId,
            sequence: sequence,
            sentAt: Date(),
            action: RemoteAction(kind: kind, favoriteId: favoriteId, brightness: brightness))

        let payload: Data
        do {
            payload = try encoder.encode(envelope)
        } catch {
            status = "Could not prepare Watch command."
            playHaptic(.failure)
            completion?()
            return
        }

        busyActionIds.insert(actionId)
        transport.sendMessageData(payload) { [weak self] responseData in
            Task { @MainActor in
                guard let self else { return }
                self.busyActionIds.remove(actionId)
                self.applyResponse(responseData)
                completion?()
            }
        } errorHandler: { [weak self] _ in
            Task { @MainActor in
                guard let self else { return }
                self.busyActionIds.remove(actionId)
                self.phoneReachable = false
                self.status = "iPhone unavailable. Command was not queued."
                self.playHaptic(.failure)
                completion?()
            }
        }
    }

    private func applyApplicationContext(_ context: [String: Any]) {
        guard let stateJson = context["stateJson"] as? String,
              let data = stateJson.data(using: .utf8) else { return }
        applyStateData(data)
    }

    private func applyResponse(_ data: Data) {
        guard let result = try? decoder.decode(RemoteProcessResult.self, from: data),
              result.schemaVersion == Self.protocolVersion else {
            status = "iPhone returned an invalid Watch response."
            playHaptic(.failure)
            return
        }

        applyState(result.state)
        status = result.message
        playHaptic(hapticType(result.haptic, succeeded: result.succeeded))
    }

    private func applyStateData(_ data: Data) {
        guard let decoded = try? decoder.decode(RemoteState.self, from: data),
              decoded.schemaVersion == Self.protocolVersion else { return }
        applyState(decoded)
    }

    private func applyState(_ value: RemoteState) {
        state = value
        status = value.companionStatus.isEmpty ? value.readinessSummary : value.companionStatus
    }

    private func showUnavailableStatus() {
        status = phoneReachable
            ? "Bring Shining Mask to the foreground before sending this command."
            : "iPhone unavailable. Command was not queued."
        playHaptic(.failure)
    }

    private func hapticType(_ name: String, succeeded: Bool) -> WKHapticType {
        switch name {
        case "Success": return .success
        case "Warning": return .retry
        case "Failure": return .failure
        default: return succeeded ? .success : .failure
        }
    }
}
