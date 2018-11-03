using AutoMapper;
using MySql.Data.MySqlClient;
using MySqlSideKicks.Win.Models;
using MySqlSideKicks.Win.Repositories;
using MySqlSideKicks.Win.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MySqlSideKicks.Win
{
    class ConnectionPresenter
    {
        private IList<Connection> _connections = new List<Connection>();
        private readonly IConnectionView _view;
        private readonly IConnectionRepository _connectionRepository;
        private Connection _currentConnection;
        private IMapper _mapper;

        public ConnectionPresenter(IConnectionView connectionView, IConnectionRepository connectionRepository)
        {
            _view = connectionView;
            _connectionRepository = connectionRepository;

            _view.Initialize += View_Initialize;
            _view.New += View_New;
            _view.Delete += View_Delete;
            _view.ConnectionSelected += View_ConnectionSelected;
            _view.ConnectionChanged += View_ConnectionChanged;
            _view.Test += View_Test;
            _view.Connect += View_Connect;

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Connection, ConnectionViewModel>();
            });

            _mapper = mapperConfiguration.CreateMapper();
        }

        private async Task View_Initialize()
        {
            _connections = await _connectionRepository.GetAll();
            _view.LoadConnectionList(_mapper.Map<IList<ConnectionViewModel>>(_connections));

            var lastUsedConnection = await _connectionRepository.GetLastUsed();
            if (lastUsedConnection != null)
            {
                _view.OpenConnection(_mapper.Map<ConnectionViewModel>(lastUsedConnection));
            }
            else
            {
                await View_New();
            }
        }

        private Task View_New()
        {
            _connections.Add(new Connection()
            {
                Name = GenerateConnectionName()
            });

            _view.LoadConnectionList(_mapper.Map<IList<ConnectionViewModel>>(_connections));

            return Task.CompletedTask;
        }

        private string GenerateConnectionName(string baseName = "Connection")
        {
            var generatedName = baseName;
            int number = 0;

            do
            {
                generatedName = $"{baseName}{++number}";
            } while (_connections.Any(c => c.Name.EqualsIgnoreCase(generatedName)));

            return generatedName;
        }

        private Task View_Delete()
        {
            _connections.Remove(_currentConnection);
            _view.LoadConnectionList(_mapper.Map<IList<ConnectionViewModel>>(_connections));

            return Task.CompletedTask;
        }

        private Task View_ConnectionChanged(ConnectionViewModel connectionViewModel)
        {
            var connection = _connections.FirstOrDefault(c => c.Guid == connectionViewModel.Guid);

            _mapper.Map(connectionViewModel, connection);

            return Task.CompletedTask;
        }

        private void View_ConnectionSelected(ConnectionViewModel connectionViewModel)
        {
            var connection = _connections.FirstOrDefault(c => c.Guid == connectionViewModel.Guid);

            if (connection == null)
            {
                return;
            }

            _view.OpenConnection(_mapper.Map<ConnectionViewModel>(connection));
            _currentConnection = connection;
        }

        private Task View_Connect()
        {
            _connectionRepository.SaveAll(_connections);
            _connectionRepository.SaveLastUsed(_currentConnection);

            _view.LoadConnectionList(_mapper.Map<IList<ConnectionViewModel>>(_connections));

            return Task.CompletedTask;
        }

        private async Task View_Test()
        {
            try
            {
                var connectionString = _currentConnection.ToMySqlConnectionString();

                using (var mySqlConnection = new MySqlConnection(connectionString))
                {
                    await mySqlConnection.OpenAsync();
                }

                _view.ShowInformationMessage("Connection Succeeded");
            }
            catch (Exception ex)
            {
                _view.ShowErrorMessage(ex.Message);
            }
        }
    }
}
