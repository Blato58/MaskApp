import SwiftUI

@main
struct MaskAppWatchApp: App {
    @StateObject private var model = RemoteSessionModel()

    var body: some Scene {
        WindowGroup {
            ContentView(model: model)
        }
    }
}
