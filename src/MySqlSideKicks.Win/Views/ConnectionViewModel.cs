using System;
using System.ComponentModel;

namespace MySqlSideKicks.Win.Models
{
    public class ConnectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _server;
        private uint _port;
        private string _userID;
        private string _password;
        private string _database;

        public Guid Guid { get; set; }
        public string Name { get => _name; set { _name = value; NotifyPropertyChanged(nameof(Name)); } }
        public string Server { get => _server; set { _server = value; NotifyPropertyChanged(nameof(Server)); } }
        public uint Port { get => _port; set { _port = value; NotifyPropertyChanged(nameof(Port)); } }
        public string UserID { get => _userID; set { _userID = value; NotifyPropertyChanged(nameof(UserID)); } }
        public string Password { get => _password; set { _password = value; NotifyPropertyChanged(nameof(Password)); } }
        public string Database { get => _database; set { _database = value; NotifyPropertyChanged(nameof(Database)); } }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
