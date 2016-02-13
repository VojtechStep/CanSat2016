using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsCode.Classes
{
    public class RecentItem
    {
        public delegate void ClickHandler(object sender, EventArgs e);
        public delegate void RemoveHandler(object sender, EventArgs e);
        public delegate void PinHandler(object sender, EventArgs e);

        private String _name;
        private String _path;
        private String _icon = "\uE155";
        private Windows.UI.Color _bg;
        private BufferPosition _pos = BufferPosition.Normal;

        private event ClickHandler _click;
        private event RemoveHandler _remove;
        private event PinHandler _pin;

        private Boolean _fixed = false;
        private Boolean _removable = true;
        private Boolean _pinnable = true;
        private Boolean _pinned = true;

        public RecentItem()
        {
            Pinned = false;
        }
        

        public virtual void OnClick(EventArgs e)
        {
            if (_click != null)
                _click(this, e);
        }

        public virtual void OnRemoveRequested(EventArgs e)
        {
            if (_removable && _remove != null)
                _remove(this, e);
        }

        public virtual void OnPinToggled(EventArgs e)
        {
            if (_pinnable && _pin != null)
                _pin(this, e);
        }

        public Boolean HasRemoveHandler { get { return _remove != null; } }
        public Boolean HasClickHandler { get { return _click != null; } }
        public Boolean HasPinHandler { get { return _pin != null; } }

        public event ClickHandler Click
        {
            add { _click += value; }
            remove { _click -= value; }
        }

        public event RemoveHandler Remove
        {
            add { _remove += value; }
            remove { _remove -= value; }
        }

        public event PinHandler PinnedToggle
        {
            add { _pin += value; }
            remove { _pin -= value; }
        }

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public String Location
        {
            get { return _path; }
            set { _path = value; }
        }


        public Windows.UI.Color Bg
        {
            get { return _bg; }
            set { _bg = value; }
        }

        public String Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        public BufferPosition Position
        {
            get { return _pos; }
            set { _pos = value; }
        }

        public Boolean Fixed
        {
            get { return _fixed; }
            set { _fixed = value; }
        }

        public Boolean Removable
        {
            get { return _removable; }
            set { _removable = value; }
        }

        public Boolean Pinnable
        {
            get { return _pinnable; }
            set { _pinnable = value; }
        }

        public Boolean Pinned
        {
            get { return _pinned; }
            set
            {
                if (_pinnable)
                {
                    _pinned = value;
                    OnPinToggled(EventArgs.Empty);
                }
            }
        }

    }
}
