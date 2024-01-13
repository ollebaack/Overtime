
namespace Overtime
{
    public partial class MyNewPage : ContentPage
    {
        public MyNewPage()
        {
            InitializeComponent();
            Content = new Label
            {
                Text = "This is my new page!",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
        }
    }
}
