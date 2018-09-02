﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CasaDoCodigo.Carrinho.Model
{
    public class RedisCarrinhoRepository : ICarrinhoRepository
    {
        private readonly ILogger<RedisCarrinhoRepository> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCarrinhoRepository(LoggerFactory loggerFactory, ConnectionMultiplexer redis)
        {
            _logger = loggerFactory.CreateLogger<RedisCarrinhoRepository>();
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<bool> DeleteCarrinhoAsync(string id)
        {
            return await _database.KeyDeleteAsync(id);
        }

        public async Task<CarrinhoCliente> GetCarrinhoAsync(string clienteId)
        {
            var data = await _database.StringGetAsync(clienteId);
            if (data.IsNullOrEmpty)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<CarrinhoCliente>(data);
        }

        public IEnumerable<string> GetUsuarios()
        {
            var server = GetServer();
            return server.Keys()?.Select(k => k.ToString());
        }

        public async Task<CarrinhoCliente> UpdateCarrinhoAsync(CarrinhoCliente carrinho)
        {
            var criado = await _database.StringSetAsync(carrinho.ClienteId, JsonConvert.SerializeObject(carrinho));
            if (!criado)
            {
                _logger.LogError("Erro ao atualizar o carrinho.");
                return null;
            }
            _logger.LogInformation("Carrinho atualizado.");
            return await GetCarrinhoAsync(carrinho.ClienteId);
        }

        private IServer GetServer()
        {
            var endpoints = _redis.GetEndPoints();
            return _redis.GetServer(endpoints.First());
        }
    }
}