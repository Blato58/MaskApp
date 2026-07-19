using Microsoft.Maui.Controls.Xaml;

namespace MaskApp.App.Resources.Strings;

[ContentProperty(nameof(Key))]
public sealed class TranslateExtension(string key) : IMarkupExtension<string>
{
    public string Key { get; set; } = key;

    public string ProvideValue(IServiceProvider serviceProvider) => AppText.Get(Key);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
