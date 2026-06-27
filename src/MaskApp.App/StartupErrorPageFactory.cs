namespace MaskApp.App;

internal static class StartupErrorPageFactory
{
    public static ContentPage Create(string title, Exception exception)
    {
        return new ContentPage
        {
            Title = title,
            Padding = 20,
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 12,
                    Children =
                    {
                        new Label
                        {
                            Text = title,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 24
                        },
                        new Label
                        {
                            Text = "The app caught a managed startup error. Send this screen or the iOS crash log back for the next fix.",
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        new Label
                        {
                            Text = exception.ToString(),
                            FontFamily = "Courier",
                            FontSize = 12,
                            LineBreakMode = LineBreakMode.WordWrap
                        }
                    }
                }
            }
        };
    }
}
