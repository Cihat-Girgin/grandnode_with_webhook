﻿using Grand.Business.Messages.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Business.Messages.Tests.Startup;

[TestClass]
public class StartupApplicationTests
{
    private StartupApplication _application;
    private IConfiguration _configuration;
    private ServiceCollection _serviceCollection;

    [TestInitialize]
    public void Init()
    {
        _serviceCollection = new ServiceCollection();
        _configuration = new ConfigurationBuilder().Build();
        _application = new StartupApplication();
    }

    [TestMethod]
    public void ConfigureServicesTest()
    {
        //Act
        _application.ConfigureServices(_serviceCollection, _configuration);
        //Assert
        Assert.IsTrue(_serviceCollection.Count > 0);
    }
}