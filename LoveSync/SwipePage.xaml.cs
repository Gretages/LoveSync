using LoveSync.ViewModels;

namespace LoveSync;

public partial class SwipePage : ContentPage
{
    public SwipePage()
    {
        InitializeComponent();
    }

    private async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (BindingContext is not SwipeViewModel viewModel)
            return;

        if (viewModel.IsFinished || !viewModel.IsSwipeEnabled)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                CardFrame.TranslationX = e.TotalX;
                CardFrame.Rotation = e.TotalX / 20;
                break;

            case GestureStatus.Completed:
                if (CardFrame.TranslationX > 100) // Like
                {
                    await CardFrame.TranslateTo(500, 0, 200);
                    viewModel.SwipeRightCommand.Execute(null);
                }
                else if (CardFrame.TranslationX < -100) // Dislike
                {
                    await CardFrame.TranslateTo(-500, 0, 200);
                    viewModel.SwipeLeftCommand.Execute(null);
                }
                else
                {
                    // Vissza az eredeti helyére:
                    await CardFrame.TranslateTo(0, 0, 120);
                }

                CardFrame.TranslationX = 0;
                CardFrame.Rotation = 0;
                break;
        }
    }
}