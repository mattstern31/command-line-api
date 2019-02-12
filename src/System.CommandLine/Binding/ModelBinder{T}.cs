﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;

namespace System.CommandLine.Binding
{
    public class ModelBinder<TModel> : ModelBinder
    {
        public ModelBinder() : base(ModelDescriptor.FromType<TModel>())
        {
        }

        public void BindPropertyFromOption<TValue>(
            Expression<Func<TModel, TValue>> property,
            IOption option)
        {
            NamedValueSources.Add(
                property.MemberTypeAndName(),
                new SpecificSymbolValueSource(option));
        }

        public void BindPropertyFromCommand<TValue>(
            Expression<Func<TModel, TValue>> property,
            ICommand command)
        {
            NamedValueSources.Add(
                property.MemberTypeAndName(),
                new SpecificSymbolValueSource(command));
        }
    }
}
