﻿/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Newtonsoft.Json;
using System;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace IdentityServer3.EntityFramework.Serialization
{
  public class ClientLite
  {
    public string ClientId { get; set; }
  }

  public class ClientConverter : JsonConverter
  {
    private readonly IClientStore _clientStore;

    public ClientConverter(IClientStore clientStore)
    {
      if (clientStore == null) throw new ArgumentNullException("clientStore");

      _clientStore = clientStore;
    }

    public override bool CanConvert(Type objectType)
    {
      return typeof(Client) == objectType;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      var source = serializer.Deserialize<ClientLite>(reader);
      var client = AsyncHelper.RunSync(async () => await _clientStore.FindClientByIdAsync(source.ClientId));
      return client;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var source = (Client)value;

      var target = new ClientLite
      {
        ClientId = source.ClientId
      };
      serializer.Serialize(writer, target);
    }
  }
}
