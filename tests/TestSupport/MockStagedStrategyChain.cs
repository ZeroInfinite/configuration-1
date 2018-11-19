﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity.TestSupport;
using Unity.Builder.Strategy;
using Unity.Strategy;

namespace Microsoft.Practices.Unity.Configuration.Tests.TestSupport
{
    /// <summary>
    /// Represents a chain of responsibility for builder strategies partitioned by stages.
    /// </summary>
    /// <typeparam name="TStageEnum">The stage enumeration to partition the strategies.</typeparam>
    public class MockStagedStrategyChain<TStageEnum> : IStagedStrategyChain<BuilderStrategy, TStageEnum>
    {
        private readonly MockStagedStrategyChain<TStageEnum> _innerChain;
        private readonly object _lockObject = new object();
        private readonly List<BuilderStrategy>[] _stages;


        /// <summary>
        /// Initialize a new instance of the <see cref="MockStagedStrategyChain{TStageEnum}"/> class.
        /// </summary>
        public MockStagedStrategyChain()
        {
            _stages = new List<BuilderStrategy>[NumberOfEnumValues()];

            for (int i = 0; i < _stages.Length; ++i)
            {
                _stages[i] = new List<BuilderStrategy>();
            }
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="MockStagedStrategyChain{TStageEnum}"/> class with an inner strategy chain to use when building.
        /// </summary>
        /// <param name="innerChain">The inner strategy chain to use first when finding strategies in the build operation.</param>
        public MockStagedStrategyChain(MockStagedStrategyChain<TStageEnum> innerChain)
            : this()
        {
            _innerChain = innerChain;
        }

        public event EventHandler<EventArgs> Invalidated = delegate {  };

        /// <summary>
        /// Adds a strategy to the chain at a particular stage.
        /// </summary>
        /// <param name="strategy">The strategy to add to the chain.</param>
        /// <param name="stage">The stage to add the strategy.</param>
        public void Add(BuilderStrategy strategy, TStageEnum stage)
        {
            lock (_lockObject)
            {
                _stages[Convert.ToInt32(stage)].Add(strategy);
            }
        }

        /// <summary>
        /// Clear the current strategy chain list.
        /// </summary>
        /// <remarks>
        /// This will not clear the inner strategy chain if this instance was created with one.
        /// </remarks>
        public void Clear()
        {
            lock (_lockObject)
            {
                foreach (List<BuilderStrategy> stage in _stages)
                {
                    stage.Clear();
                }
            }
        }

        /// <summary>
        /// Makes a strategy chain based on this instance.
        /// </summary>
        /// <returns>A new <see cref="MockStrategyChain"/>.</returns>
        public IStrategyChain MakeStrategyChain()
        {
            lock (_lockObject)
            {
                var result = new MockStrategyChain();

                for (int index = 0; index < _stages.Length; ++index)
                {
                    FillStrategyChain(result, index);
                }

                return result;
            }
        }

        private void FillStrategyChain(MockStrategyChain chain, int index)
        {
            lock (_lockObject)
            {
                if (_innerChain != null)
                {
                    _innerChain.FillStrategyChain(chain, index);
                }
                chain.AddRange(_stages[index]);
            }
        }

        private static int NumberOfEnumValues()
        {
            return typeof(TStageEnum).GetTypeInfo().DeclaredFields.Count(f => f.IsPublic && f.IsStatic);
        }

        public IEnumerator<BuilderStrategy> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
