using Microsoft.Maui.Controls.Xaml;

namespace MaskApp.App.Resources.Strings;

[ContentProperty(nameof(Key))]
[AcceptEmptyServiceProvider]
public sealed class TranslateExtension : IMarkupExtension<string>
{
    public TranslateExtension()
    {
    }

    public TranslateExtension(string key) => Key = key;

    public string Key { get; set; } = string.Empty;

    public string ProvideValue(IServiceProvider serviceProvider) => AppText.Get(Key);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
