﻿//-----------------------------------------------------------------------------
// FILE:	    Main.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright (c) 2016-2019 by neonFORGE, LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

//-----------------------------------------------------------------------------
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// This sample demonstrates how to write a very simple workflow, register it
// with Temporal, and then execute it and obtain the result.  The workflow 
// argument is a persons name and the result is a greeting.
//
// Requirements:
// -------------
// You'll need to have Docker installed as described in the repo README.md 
// have Temporal running locally via:
//
//      docker run --detach --name temporal-dev -p 7933-7939:7933-7939 -p 8088:8088 nkubeio/temporal-dev
//
// You can view that Temporal portal at:
//
//      http://localhost:8088/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

using Neon.Temporal;

namespace hello_workflow
{
    [WorkflowInterface(TaskQueue = "hello-tasks")]
    public interface IHelloWorkflow : IWorkflow
    {
        [WorkflowMethod]
        Task<string> HelloAsync(string name);
    }

    [Workflow(AutoRegister = true)]
    public class HelloWorkflow : WorkflowBase, IHelloWorkflow
    {
        [WorkflowMethod]
        public async Task<string> HelloAsync(string name)
        {
            return await Task.FromResult($"Hello {name}!");
        }
    }

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var settings = new TemporalSettings("localhost:7233")
                {
                    Namespace       = "test-namespace",
                    CreateNamespace = true
                };

                using (var client = await TemporalClient.ConnectAsync(settings))
                {
                    using (var worker = await client.NewWorkerAsync())
                    {
                        await worker.RegisterAssemblyAsync(Assembly.GetExecutingAssembly());
                        await worker.StartAsync();

                        var stub = client.NewWorkflowStub<IHelloWorkflow>();
                        var result = await stub.HelloAsync("Sally");

                        Console.WriteLine($"RESULT: {result}");
                    }
                }
            }
            catch (ConnectException)
            {
                Console.Error.WriteLine("Cannot connect to Temporal.  Be sure you've started a");
                Console.Error.WriteLine("local Temporal Docker container via:");
                Console.Error.WriteLine();
                Console.Error.WriteLine("docker run --detach --name temporal-dev -p 7933-7939:7933-7939 -p 8088:8088 nkubeio/temporal-dev");
            }
        }
    }
}
