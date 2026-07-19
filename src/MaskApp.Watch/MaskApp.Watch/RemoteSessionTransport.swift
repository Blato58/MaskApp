import Foundation
import WatchConnectivity

protocol RemoteSessionTransport: AnyObject {
    var isReachable: Bool { get }
    var receivedApplicationContext: [String: Any] { get }
    var onApplicationContext: (([String: Any]) -> Void)? { get set }
    var onReachabilityChanged: ((Bool) -> Void)? { get set }

    func activate()
    func sendMessageData(
        _ data: Data,
        replyHandler: @escaping (Data) -> Void,
        errorHandler: @escaping (Error) -> Void)
}

final class WatchConnectivityTransport: NSObject, RemoteSessionTransport, WCSessionDelegate {
    private let session: WCSession

    var onApplicationContext: (([String: Any]) -> Void)?
    var onReachabilityChanged: ((Bool) -> Void)?

    override init() {
        session = .default
        super.init()
    }

    var isReachable: Bool { session.isReachable }

    var receivedApplicationContext: [String: Any] { session.receivedApplicationContext }

    func activate() {
        guard WCSession.isSupported() else { return }
        session.delegate = self
        session.activate()
    }

    func sendMessageData(
        _ data: Data,
        replyHandler: @escaping (Data) -> Void,
        errorHandler: @escaping (Error) -> Void)
    {
        session.sendMessageData(data, replyHandler: replyHandler, errorHandler: errorHandler)
    }

    func session(
        _ session: WCSession,
        activationDidCompleteWith activationState: WCSessionActivationState,
        error: Error?)
    {
        onReachabilityChanged?(session.isReachable)
        if activationState == .activated, !session.receivedApplicationContext.isEmpty {
            onApplicationContext?(session.receivedApplicationContext)
        }
    }

    func sessionReachabilityDidChange(_ session: WCSession) {
        onReachabilityChanged?(session.isReachable)
    }

    func session(_ session: WCSession, didReceiveApplicationContext applicationContext: [String: Any]) {
        onApplicationContext?(applicationContext)
    }
}
