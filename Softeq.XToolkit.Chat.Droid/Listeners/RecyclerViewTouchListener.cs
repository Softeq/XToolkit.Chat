// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Windows.Input;
using Android.Views;

namespace Softeq.XToolkit.Chat.Droid.Listeners
{
    public class RecyclerViewTouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        private readonly ICommand _tappedCommand;

        public RecyclerViewTouchListener(ICommand tappedCommand)
        {
            _tappedCommand = tappedCommand;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            _tappedCommand.Execute(null);

            return false;
        }
    }
}
