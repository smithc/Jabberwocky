﻿using System;
using Autofac;
using Jabberwocky.Glass.Autofac.DependencyInjection.Factories;
using Jabberwocky.Glass.Autofac.Tests.Pipelines.Processors;
using Jabberwocky.Glass.Autofac.Util;
using NSubstitute;
using NUnit.Framework;

namespace Jabberwocky.Glass.Autofac.Tests.Pipelines.Pipelines
{
	[TestFixture]
	public class AutofacProcessorFactoryTests
	{
		private AutofacSitecoreFactory _factory;
		private static readonly string TestProcessorFQN = typeof (TestProcessor).AssemblyQualifiedName;

		[SetUp]
		public void TestSetup()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<TestProcessor>().AsSelf();
			builder.RegisterType<object>().AsSelf(); // dependency of TestProcessor

			AutofacConfig.ServiceLocator = builder.Build();

			// SUT
			_factory = new AutofacSitecoreFactory();
        }

		[TestFixtureTearDown]
		public void TestCleanup()
		{
			AutofacConfig.ServiceLocator = null;
		}

		[Test]
		public void GetObject_NullType_ReturnsNull()
		{
			var service = _factory.GetObject(null);

			Assert.IsNull(service);
		}

		[Test]
		public void GetObject_EmptyStringType_ReturnsNull()
		{
			var service = _factory.GetObject(string.Empty);

			Assert.IsNull(service);
		}

		[Test]
		public void GetObject_InvalidType_ReturnsNull()
		{
			var service = _factory.GetObject("I absolutely don't exist");

			Assert.IsNull(service);
		}

		[Test]
		public void GetObject_ThrowsResolvingObject_DisposesLifetime()
		{
			// Arrange... override default settings
			var mockScope = Substitute.For<ILifetimeScope>();
			var mockContainer = Substitute.For<IContainer>();

			mockContainer.BeginLifetimeScope(Arg.Any<Action<object>>()).ReturnsForAnyArgs(mockScope);

			AutofacConfig.ServiceLocator = mockContainer;

			// Act

			var service = _factory.GetObject(TestProcessorFQN);

			// Assert

			Assert.IsNull(service);
			mockScope.Received().Dispose();
		}
	}
}
