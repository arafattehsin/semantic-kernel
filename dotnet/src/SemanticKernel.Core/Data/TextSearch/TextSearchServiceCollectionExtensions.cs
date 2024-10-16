﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Extension methods to register <see cref="ITextSearch"/> for use with <see cref="IServiceCollection"/>.
/// </summary>
[Experimental("SKEXP0001")]
public static class TextSearchServiceCollectionExtensions
{
    /// <summary>
    /// Register a <see cref="VectorStoreTextSearch{TRecord}"/> instance with the specified service ID.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register the <see cref="ITextSearch"/> on.</param>
    /// <param name="stringMapper"><see cref="ITextSearchStringMapper" /> instance that can map a TRecord to a <see cref="string"/></param>
    /// <param name="resultMapper"><see cref="ITextSearchResultMapper" /> instance that can map a TRecord to a <see cref="TextSearchResult"/></param>
    /// <param name="options">Options used to construct an instance of <see cref="VectorStoreTextSearch{TRecord}"/></param>
    /// <param name="serviceId">An optional service id to use as the service key.</param>
    public static IServiceCollection AddVectorStoreTextSearch<TRecord>(
        this IServiceCollection services,
        ITextSearchStringMapper? stringMapper = null,
        ITextSearchResultMapper? resultMapper = null,
        VectorStoreTextSearchOptions? options = null,
        string? serviceId = default)
    {
        // If we are not constructing the dependent services, add the VectorStoreTextSearch as transient, since we
        // cannot make assumptions about how dependent services are being managed.
        services.AddKeyedTransient<VectorStoreTextSearch<TRecord>>(
            serviceId,
            (sp, obj) =>
            {
                stringMapper ??= sp.GetService<ITextSearchStringMapper>();
                resultMapper ??= sp.GetService<ITextSearchResultMapper>();
                options ??= sp.GetService<VectorStoreTextSearchOptions>();

                var vectorizableTextSearch = sp.GetService<IVectorizableTextSearch<TRecord>>();
                if (vectorizableTextSearch is not null)
                {
                    return new VectorStoreTextSearch<TRecord>(
                        vectorizableTextSearch,
                        stringMapper,
                        resultMapper,
                        options);
                }

                var vectorizedSearch = sp.GetService<IVectorizedSearch<TRecord>>();
                var generationService = sp.GetService<ITextEmbeddingGenerationService>();
                if (vectorizedSearch is not null && generationService is not null)
                {
                    return new VectorStoreTextSearch<TRecord>(
                        vectorizedSearch,
                        generationService,
                        stringMapper,
                        resultMapper,
                        options);
                }

                throw new InvalidOperationException("No IVectorizableTextSearch<TRecord> or IVectorizedSearch<TRecord> and ITextEmbeddingGenerationService registered.");
            });

        return services;
    }

    /// <summary>
    /// Register a <see cref="VectorStoreTextSearch{TRecord}"/> instance with the specified service ID.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register the <see cref="ITextSearch"/> on.</param>
    /// <param name="vectorizableTextSearchServiceId">Service id of the <see cref="IVectorizableTextSearch{TRecord}"/> to use.</param>
    /// <param name="stringMapper"><see cref="ITextSearchStringMapper" /> instance that can map a TRecord to a <see cref="string"/></param>
    /// <param name="resultMapper"><see cref="ITextSearchResultMapper" /> instance that can map a TRecord to a <see cref="TextSearchResult"/></param>
    /// <param name="options">Options used to construct an instance of <see cref="VectorStoreTextSearch{TRecord}"/></param>
    /// <param name="serviceId">An optional service id to use as the service key.</param>
    public static IServiceCollection AddVectorStoreTextSearch<TRecord>(
        this IServiceCollection services,
        string vectorizableTextSearchServiceId,
        ITextSearchStringMapper? stringMapper = null,
        ITextSearchResultMapper? resultMapper = null,
        VectorStoreTextSearchOptions? options = null,
        string? serviceId = default)
        where TRecord : class
    {
        // If we are not constructing the dependent services, add the VectorStoreTextSearch as transient, since we
        // cannot make assumptions about how dependent services are being managed.
        services.AddKeyedTransient<VectorStoreTextSearch<TRecord>>(
            serviceId,
            (sp, obj) =>
            {
                stringMapper ??= sp.GetService<ITextSearchStringMapper>();
                resultMapper ??= sp.GetService<ITextSearchResultMapper>();
                options ??= sp.GetService<VectorStoreTextSearchOptions>();

                var vectorizableTextSearch = sp.GetKeyedService<IVectorizableTextSearch<TRecord>>(vectorizableTextSearchServiceId);
                if (vectorizableTextSearch is not null)
                {
                    return new VectorStoreTextSearch<TRecord>(
                        vectorizableTextSearch,
                        stringMapper,
                        resultMapper,
                        options);
                }

                throw new InvalidOperationException($"No IVectorizableTextSearch<TRecord> for service id {vectorizableTextSearchServiceId} registered.");
            });

        return services;
    }

    /// <summary>
    /// Register a <see cref="VectorStoreTextSearch{TRecord}"/> instance with the specified service ID.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register the <see cref="ITextSearch"/> on.</param>
    /// <param name="vectorizedSearchServiceId">Service id of the <see cref="IVectorizedSearch{TRecord}"/> to use.</param>
    /// <param name="textEmbeddingGenerationServiceId">Service id of the <see cref="ITextEmbeddingGenerationService"/> to use.</param>
    /// <param name="stringMapper"><see cref="ITextSearchStringMapper" /> instance that can map a TRecord to a <see cref="string"/></param>
    /// <param name="resultMapper"><see cref="ITextSearchResultMapper" /> instance that can map a TRecord to a <see cref="TextSearchResult"/></param>
    /// <param name="options">Options used to construct an instance of <see cref="VectorStoreTextSearch{TRecord}"/></param>
    /// <param name="serviceId">An optional service id to use as the service key.</param>
    public static IServiceCollection AddVectorStoreTextSearch<TRecord>(
        this IServiceCollection services,
        string vectorizedSearchServiceId,
        string textEmbeddingGenerationServiceId,
        ITextSearchStringMapper? stringMapper = null,
        ITextSearchResultMapper? resultMapper = null,
        VectorStoreTextSearchOptions? options = null,
        string? serviceId = default)
        where TRecord : class
    {
        // If we are not constructing the dependent services, add the VectorStoreTextSearch as transient, since we
        // cannot make assumptions about how dependent services are being managed.
        services.AddKeyedTransient<VectorStoreTextSearch<TRecord>>(
            serviceId,
            (sp, obj) =>
            {
                stringMapper ??= sp.GetService<ITextSearchStringMapper>();
                resultMapper ??= sp.GetService<ITextSearchResultMapper>();
                options ??= sp.GetService<VectorStoreTextSearchOptions>();

                var vectorizedSearch = sp.GetKeyedService<IVectorizedSearch<TRecord>>(vectorizedSearchServiceId);
                if (vectorizedSearch is null)
                {
                    throw new InvalidOperationException($"No IVectorizedSearch<TRecord> for service id {vectorizedSearchServiceId} registered.");
                }

                var generationService = sp.GetKeyedService<ITextEmbeddingGenerationService>(textEmbeddingGenerationServiceId);
                if (vectorizedSearch is not null && generationService is not null)
                {
                    return new VectorStoreTextSearch<TRecord>(
                        vectorizedSearch,
                        generationService,
                        stringMapper,
                        resultMapper,
                        options);
                }

                throw new InvalidOperationException($"No ITextEmbeddingGenerationService for service id {textEmbeddingGenerationServiceId} registered.");
            });

        return services;
    }
}