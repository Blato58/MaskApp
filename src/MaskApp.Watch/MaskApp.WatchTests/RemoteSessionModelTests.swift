import Foundation
import XCTest
@testable import MaskApp_Watch

@MainActor
final class RemoteSessionModelTests: XCTestCase {
    func testCachedContextPublishesPhoneAndMaskState() throws {
        let transport = FakeTransport()
        transport.reachable = true
        transport.context = ["stateJson": try Self.stateJson(phoneForeground: true)]

        let model = makeModel(transport: transport)

        XCTAssertTrue(model.canSend("TriggerCurrentCue"))
        XCTAssertEqual("Mask connected", model.statusTitle)
        XCTAssertEqual("Opening", model.currentCueLabel)
        XCTAssertEqual(["favorite-1"], model.favorites.map(\.id))
    }

    func testUnreachableActionIsNotQueued() {
        let transport = FakeTransport()
        let model = makeModel(transport: transport)

        model.sendAction("Blackout")

        XCTAssertTrue(transport.sentMessages.isEmpty)
        XCTAssertTrue(model.status.contains("not queued"))
    }

    func testActionUsesVersionedEnvelopeContract() throws {
        let transport = FakeTransport()
        transport.reachable = true
        transport.context = ["stateJson": try Self.stateJson(phoneForeground: true)]
        let model = makeModel(transport: transport)

        model.sendAction("Blackout")

        let payload = try XCTUnwrap(transport.sentMessages.last)
        let message = try XCTUnwrap(
            JSONSerialization.jsonObject(with: payload) as? [String: Any])
        let action = try XCTUnwrap(message["action"] as? [String: Any])
        XCTAssertEqual(message["schemaVersion"] as? Int, 1)
        XCTAssertEqual(message["sequence"] as? Int, 1)
        XCTAssertNotNil(message["messageId"] as? String)
        XCTAssertNotNil(message["senderInstanceId"] as? String)
        XCTAssertNotNil(message["sentAt"] as? String)
        XCTAssertEqual(action["kind"] as? String, "Blackout")
    }

    private func makeModel(transport: FakeTransport) -> RemoteSessionModel {
        let suite = "RemoteSessionModelTests.\(UUID().uuidString)"
        let defaults = UserDefaults(suiteName: suite)!
        defaults.removePersistentDomain(forName: suite)
        return RemoteSessionModel(
            transport: transport,
            userDefaults: defaults,
            playHaptic: { _ in })
    }

    private static func stateJson(phoneForeground: Bool) throws -> String {
        let state: [String: Any] = [
            "schemaVersion": 1,
            "revision": 4,
            "generatedAt": "2026-07-19T12:00:00+00:00",
            "staleAfterSeconds": 15,
            "positionKind": "Setlist",
            "positionTitle": "Main set",
            "positionText": "Cue 1 of 3",
            "currentCueId": "opening",
            "currentCueLabel": "Opening",
            "nextCueLabel": "Drop",
            "maskConnected": true,
            "maskConnectionText": "Connected",
            "readinessStatus": "READY",
            "readinessSummary": "Ready to perform.",
            "watchReachable": true,
            "companionStatus": "Apple Watch is reachable.",
            "foregroundExecutionRequired": true,
            "phoneForeground": phoneForeground,
            "favorites": [
                [
                    "id": "favorite-1",
                    "label": "LOL",
                    "kind": "Text",
                    "colorHex": "#A78BFA"
                ]
            ]
        ]
        let data = try JSONSerialization.data(withJSONObject: state)
        return String(decoding: data, as: UTF8.self)
    }
}

private final class FakeTransport: RemoteSessionTransport {
    var reachable = false
    var context: [String: Any] = [:]
    var nextReply = Data()
    var sentMessages: [Data] = []
    var onApplicationContext: (([String: Any]) -> Void)?
    var onReachabilityChanged: ((Bool) -> Void)?

    var isReachable: Bool { reachable }
    var receivedApplicationContext: [String: Any] { context }

    func activate() {}

    func sendMessageData(
        _ data: Data,
        replyHandler: @escaping (Data) -> Void,
        errorHandler: @escaping (Error) -> Void)
    {
        sentMessages.append(data)
        replyHandler(nextReply)
    }
}
