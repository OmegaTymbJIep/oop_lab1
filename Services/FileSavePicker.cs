using Foundation;
using Lab1.Core.Services;
using UIKit;
using static UIKit.UIApplication;

namespace Lab1.Services;

public class FileSavePicker: IFileSavePicker
{
    public Task<string> PickAsync(string defaultFileName)
    {
        var tcs = new TaskCompletionSource<string>();

        var tempFilePath = Path.Combine(Path.GetTempPath(), defaultFileName);
        File.WriteAllText(tempFilePath, string.Empty);

        var documentUrl = NSUrl.FromFilename(tempFilePath);

        var documentPicker = new UIDocumentPickerViewController(new NSUrl[] { documentUrl }, UIDocumentPickerMode.ExportToService)
        {
            AllowsMultipleSelection = false,
            ShouldShowFileExtensions = true
        };

        documentPicker.DidPickDocumentAtUrls += (sender, e) =>
        {
            var url = e.Urls?.FirstOrDefault();
            if (url != null)
            {
                if (url.Path != null) tcs.SetResult(url.Path);
            }
            else
            {
                tcs.SetResult(null!);
            }
        };

        documentPicker.WasCancelled += (sender, e) =>
        {
            tcs.SetResult(null!);
        };

        var viewController = GetCurrentViewController();
        viewController.PresentViewController(documentPicker, true, null);

        return tcs.Task;
    }

    private static UIViewController GetCurrentViewController()
    {
        var window = SharedApplication.KeyWindow;

        if (window == null)
        {
            throw new InvalidOperationException("There's no current active window");
        }

        var viewController = window.RootViewController;
        if (viewController == null)
        {
            throw new InvalidOperationException("There's no root view controller");
        }

        while (viewController.PresentedViewController != null)
        {
            viewController = viewController.PresentedViewController;
        }

        return viewController;
    }
}