﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lykke.Job.CandlesProducer.Core.Domain
{
    public interface ICandlesProducerBlobRepository
    {
        [CanBeNull]
        T Read<T>(string blobContainer, string key);
        Task Write<T>(string blobContainer, string key, T obj);
        [ItemCanBeNull]
        Task<T> ReadAsync<T>(string blobContainer, string key);
    }
}
