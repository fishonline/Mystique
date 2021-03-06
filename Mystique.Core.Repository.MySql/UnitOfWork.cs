﻿using Microsoft.Extensions.Options;
using Mystique.Core.Models;
using Mystique.Core.Repositories;
using System.Collections.Generic;
using MySqlClient = MySql.Data.MySqlClient;

namespace Mystique.Core.Repository.MySql
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbHelper _dbHelper = null;
        private readonly string _connectionString = string.Empty;
        private IPluginRepository _pluginRepository = null;
        private ISiteRepository _siteRepository = null;
        private readonly List<Command> _commands;

        public UnitOfWork(IOptions<ConnectionStringSetting> connectionStringAccessor)
        {
            _commands = new List<Command>();
            _connectionString = connectionStringAccessor.Value.ConnectionString;
            _dbHelper = new DbHelper(_connectionString);
        }

        public IPluginRepository PluginRepository
        {
            get
            {
                if (_pluginRepository == null)
                {
                    _pluginRepository = new PluginRepository(_dbHelper, _commands);
                }

                return _pluginRepository;
            }
        }

        public ISiteRepository SiteRepository
        {
            get
            {
                if (_siteRepository == null)
                {
                    _siteRepository = new SiteRepository(_dbHelper, _commands);
                }

                return _siteRepository;
            }
        }

        public void Commit()
        {
            _dbHelper.ExecuteNonQuery(_commands);
        }

        public bool CheckDatabase()
        {
            object o = _dbHelper.ExecuteScalarWithObjReturn("SELECT `Value` FROM SiteSettings WHERE `Key` = @key", new List<MySqlClient.MySqlParameter> {
               new MySqlClient.MySqlParameter { ParameterName = "@key", Value = "SYSTEM_INSTALLED"}
            }.ToArray());

            return (o != null && o.ToString() == "1");
        }

        public void MarkAsInstalled()
        {
            _dbHelper.ExecuteNonQuery("UPDATE SiteSettings SET `Value`='1' WHERE `Key`=@key", new List<MySqlClient.MySqlParameter> {
               new MySqlClient.MySqlParameter { ParameterName = "@key", Value = "SYSTEM_INSTALLED"}
            }.ToArray());
        }
    }
}
