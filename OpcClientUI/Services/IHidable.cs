using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpcClientUI.Services
{
    public interface IHidable
    {
        public bool IsVisible { get; set; }
        public void ChangeVisiblity(bool isVisible);

    }
}
