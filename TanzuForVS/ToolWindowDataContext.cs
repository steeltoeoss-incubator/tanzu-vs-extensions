﻿namespace TanzuForVS
{
    using System.ComponentModel;

    public class ToolWindowDataContext : INotifyPropertyChanged
    {
        private bool hasErrors = false;
        private string errorMessage = string.Empty;
        private bool isLoggedIn = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasErrors
        {
            get
            {
                return this.hasErrors;
            }

            set
            {
                this.hasErrors = value;
                this.RaisePropertyChangedEvent("HasErrors");
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }

            set
            {
                this.errorMessage = value;
                this.RaisePropertyChangedEvent("ErrorMessage");
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return isLoggedIn;
            }

            set
            {
                if (value == true && this.HasErrors == false)
                {
                    this.isLoggedIn = true;
                }
                else
                {
                    this.isLoggedIn = false;
                }

                this.RaisePropertyChangedEvent("IsLoggedIn");
            }
        }

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}