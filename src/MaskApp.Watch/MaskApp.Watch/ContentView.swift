import SwiftUI

struct ContentView: View {
    @ObservedObject var model: RemoteSessionModel

    private let columns = [GridItem(.flexible()), GridItem(.flexible())]

    var body: some View {
        ScrollView {
            VStack(spacing: 10) {
                statusCard
                emergencyControls
                cueControls
                brightnessControl
                favoriteControls
            }
            .padding(.horizontal, 6)
            .padding(.bottom, 8)
        }
        .navigationTitle("Shining Mask")
        .onAppear { model.refreshCachedState() }
    }

    private var statusCard: some View {
        VStack(alignment: .leading, spacing: 5) {
            HStack(spacing: 7) {
                Image(systemName: model.statusSymbol)
                    .foregroundStyle(statusColor)
                Text(model.statusTitle)
                    .font(.headline)
                Spacer(minLength: 0)
            }
            Text(model.positionTitle)
                .font(.caption.bold())
                .lineLimit(1)
            Text(model.positionText)
                .font(.caption2)
                .foregroundStyle(.secondary)
                .lineLimit(2)
            Text(model.status)
                .font(.caption2)
                .foregroundStyle(.secondary)
                .lineLimit(2)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(10)
        .background(.thinMaterial, in: RoundedRectangle(cornerRadius: 14))
        .accessibilityElement(children: .combine)
    }

    private var emergencyControls: some View {
        HStack(spacing: 7) {
            Button {
                model.sendAction("Stop")
            } label: {
                Label("STOP", systemImage: "stop.fill")
                    .frame(maxWidth: .infinity, minHeight: 38)
            }
            .buttonStyle(.bordered)
            .disabled(!model.canSend("Stop"))

            Button {
                model.sendAction("Blackout")
            } label: {
                Label("BLACKOUT", systemImage: "moon.fill")
                    .frame(maxWidth: .infinity, minHeight: 38)
            }
            .buttonStyle(.borderedProminent)
            .tint(.red)
            .disabled(!model.canSend("Blackout"))
            .accessibilityHint("Stops active output and blacks out the connected mask")
        }
    }

    private var cueControls: some View {
        VStack(spacing: 6) {
            HStack(spacing: 6) {
                Button {
                    model.sendAction("PreviousCue")
                } label: {
                    Image(systemName: "backward.end.fill")
                        .frame(maxWidth: .infinity, minHeight: 34)
                }
                .disabled(!model.canSend("PreviousCue"))
                .accessibilityLabel("Previous cue")

                Button {
                    model.sendAction("TriggerCurrentCue")
                } label: {
                    Image(systemName: "play.fill")
                        .frame(maxWidth: .infinity, minHeight: 34)
                }
                .buttonStyle(.borderedProminent)
                .disabled(!model.canSend("TriggerCurrentCue"))
                .accessibilityLabel("Trigger current cue")

                Button {
                    model.sendAction("NextCue")
                } label: {
                    Image(systemName: "forward.end.fill")
                        .frame(maxWidth: .infinity, minHeight: 34)
                }
                .disabled(!model.canSend("NextCue"))
                .accessibilityLabel("Next cue")
            }
            Text(model.currentCueLabel)
                .font(.caption.bold())
                .lineLimit(1)
            if !model.nextCueLabel.isEmpty {
                Text("Next: \(model.nextCueLabel)")
                    .font(.caption2)
                    .foregroundStyle(.secondary)
                    .lineLimit(1)
            }
        }
    }

    private var brightnessControl: some View {
        VStack(spacing: 5) {
            HStack {
                Label("Brightness", systemImage: "sun.max")
                    .font(.caption)
                Spacer()
                Button("60%") { model.restoreBrightness() }
                    .buttonStyle(.borderless)
                    .font(.caption.bold())
                    .disabled(!model.canSend("SetBrightness"))
                Text("\(Int(model.brightness.rounded()))%")
                    .font(.caption.monospacedDigit())
            }
            RoundedRectangle(cornerRadius: 12)
                .fill(.thinMaterial)
                .frame(height: 42)
                .overlay {
                    ProgressView(value: model.brightness, total: 100)
                        .tint(.yellow)
                        .padding(.horizontal, 12)
                }
                .focusable(true)
                .digitalCrownRotation(
                    $model.brightness,
                    from: 1,
                    through: 100,
                    by: 1,
                    sensitivity: .medium,
                    isContinuous: true,
                    isHapticFeedbackEnabled: true)
                .onChange(of: model.brightness) { _ in model.scheduleBrightnessSend() }
                .accessibilityLabel("Mask brightness")
                .accessibilityValue("\(Int(model.brightness.rounded())) percent")
                .accessibilityHint("Turn the Digital Crown to change brightness")
        }
        .opacity(model.canSend("SetBrightness") ? 1 : 0.5)
        .allowsHitTesting(model.canSend("SetBrightness"))
    }

    @ViewBuilder
    private var favoriteControls: some View {
        if !model.favorites.isEmpty {
            VStack(alignment: .leading, spacing: 6) {
                Text("Favorites")
                    .font(.caption)
                    .foregroundStyle(.secondary)
                LazyVGrid(columns: columns, spacing: 6) {
                    ForEach(model.favorites) { favorite in
                        Button(favorite.label) {
                            model.sendFavorite(favorite)
                        }
                        .buttonStyle(.bordered)
                        .font(.caption.bold())
                        .frame(minHeight: 38)
                        .disabled(!model.canSend("favorite:\(favorite.id)"))
                        .accessibilityLabel("Trigger favorite \(favorite.label)")
                    }
                }
            }
        }
    }

    private var statusColor: Color {
        switch model.statusColorName {
        case "green": .green
        case "orange": .orange
        default: .gray
        }
    }
}

#Preview {
    ContentView(model: RemoteSessionModel())
}
