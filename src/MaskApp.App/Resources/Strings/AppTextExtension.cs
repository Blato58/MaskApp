using Microsoft.Maui.Controls.Xaml;

namespace MaskApp.App.Resources.Strings;

[ContentProperty(nameof(Key))]
[AcceptEmptyServiceProvider]
public sealed class AppTextExtension : IMarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public object ProvideValue(IServiceProvider serviceProvider) => AppText.Get(Key);
}
