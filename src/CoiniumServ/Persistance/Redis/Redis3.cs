﻿#region License
// 
//     CoiniumServ - Crypto Currency Mining Pool Server Software
//     Copyright (C) 2013 - 2014, CoiniumServ Project - http://www.coinium.org
//     http://www.coiniumserv.com - https://github.com/CoiniumServ/CoiniumServ
// 
//     This software is dual-licensed: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//    
//     For the terms of this license, see licenses/gpl_v3.txt.
// 
//     Alternatively, you can license this software under a commercial
//     license or white-label it as set out in licenses/commercial.txt.
// 
#endregion

using System;
using System.Collections.Generic;
using CoiniumServ.Payments;
using CoiniumServ.Persistance.Blocks;
using CoiniumServ.Pools.Config;
using CoiniumServ.Shares;
using ctstone.Redis;
using Serilog;

namespace CoiniumServ.Persistance.Redis
{
    public class Redis3:IStorage, IRedis
    {
        public bool IsEnabled { get; private set; }
        public bool IsConnected { get { return _client != null && _client.Connected; } }

        private readonly Version _requiredMinimumVersion = new Version(2, 6);
        private readonly IRedisConfig _redisConfig;
        private readonly IPoolConfig _poolConfig;

        private RedisClient _client;

        private readonly ILogger _logger;

        public Redis3(PoolConfig poolConfig)
        {
            _logger = Log.ForContext<Redis3>().ForContext("Component", poolConfig.Coin.Name);

            _poolConfig = poolConfig; // the pool config.
            _redisConfig = (IRedisConfig)poolConfig.Storage;

            IsEnabled = _redisConfig.Enabled;

            if (IsEnabled)
                Initialize();
        }


        public void AddShare(IShare share)
        {
            throw new NotImplementedException();
        }

        public void AddBlock(IShare share)
        {
            throw new NotImplementedException();
        }

        public void SetRemainingBalances(IList<IWorkerBalance> workerBalances)
        {
            throw new NotImplementedException();
        }

        public void DeleteShares(IPaymentRound round)
        {
            throw new NotImplementedException();
        }

        public void MoveSharesToCurrentRound(IPaymentRound round)
        {
            throw new NotImplementedException();
        }

        public void MoveBlock(IPaymentRound round)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, int> GetBlockCounts()
        {
            throw new NotImplementedException();
        }

        public void DeleteExpiredHashrateData(int until)
        {
            if (!IsEnabled || !IsConnected)
                return;

            var key = string.Format("{0}:hashrate", _poolConfig.Coin.Name.ToLower());

            _client.ZRemRangeByScore(key, double.NegativeInfinity, until);
        }

        public IDictionary<string, double> GetHashrateData(int since)
        {
            throw new NotImplementedException();
        }

        public IList<IPendingBlock> GetPendingBlocks()
        {
            throw new NotImplementedException();
        }

        public IDictionary<uint, IPersistedBlock> GetAllBlocks()
        {
            throw new NotImplementedException();
        }

        public Dictionary<uint, Dictionary<string, double>> GetSharesForRounds(IList<IPaymentRound> rounds)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, double> GetPreviousBalances()
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
            try
            {
                // create the connection
                _client = new RedisClient(_redisConfig.Host, _redisConfig.Port, 0);

                // select the database
                _client.Select((uint) _redisConfig.DatabaseId);

                // authenticate if needed.
                if (!string.IsNullOrEmpty(_redisConfig.Password))
                    _client.Auth(_redisConfig.Password);

                // check the version
                var version = GetVersion();
                if (version < _requiredMinimumVersion)
                    throw new Exception(string.Format("You are using redis version {0}, minimum required version is 2.6", version));

                _logger.Information("Storage initialized: {0:l}:{1}, v{2:l}.", _redisConfig.Host, _redisConfig.Port, version);
            }
            catch (Exception e)
            {
                _logger.Error("Storage initialization failed: {0:l}:{1} - {2:l}", _redisConfig.Host, _redisConfig.Port, e.Message);
            }
        }

        private Version GetVersion()
        {
            Version version = null;
            var info = _client.Info("server");

            var parts = info.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var part in parts)
            {
                var data = part.Split(':');

                if (data[0] != "redis_version")
                    continue;

                version = new Version(data[1]);
            }

            return version;
        }
    }
}
