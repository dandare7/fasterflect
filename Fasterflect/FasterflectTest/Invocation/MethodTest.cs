﻿#region License

// Copyright 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion

using System;
using System.Linq;
using Fasterflect;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FasterflectTest.Invocation
{
    [TestClass]
    public class MethodTest : BaseInvocationTest
    {
        [TestMethod]
        public void TestInvokeInstanceMethod()
        {
            RunWith( ( object person ) =>
               {
                   var elements = new[] { 1d, 2d, 3d, 4d, 5d };
                   elements.ForEach( element => person.Invoke( "Walk", element ) );
                   Assert.AreEqual( elements.Sum(), person.GetFieldValue( "metersTravelled" ) );
               } );
        }

        [TestMethod]
        public void TestInvokeInstanceMethodViaMethodInfo()
        {
            RunWith( ( object person ) =>
               {
                   var elements = new[] { 1d, 2d, 3d, 4d, 5d };
                   var methodInfo = person.UnwrapIfWrapped().GetType().Method( "Walk", Flags.InstanceCriteria, new [] { typeof(int) } );
                   elements.ForEach( element => methodInfo.Invoke( person, element ) );
                   Assert.AreEqual( elements.Sum(), person.GetFieldValue( "metersTravelled" ) );
               } );
        }

        [TestMethod]
        public void TestInvokeWithCoVariantReturnAndParamType()
        {
            var person = PersonType.CreateInstance();
            var friend = EmployeeType.CreateInstance();
            var result = person.Invoke( "AddFriend", friend );
            Assert.AreSame( friend, result );
        }

        [TestMethod]
        public void TestInvokeMethodWithOutArgument()
        {
            RunWith( ( object person ) =>
               {
                   var arguments = new object[] { 10d, null };
                   person.Invoke( "Walk", new[] { typeof(double), typeof(double).MakeByRefType() }, arguments );
                   Assert.AreEqual( person.GetFieldValue( "metersTravelled" ), arguments[ 1 ] );
               } );
        }

        [TestMethod]
        public void TestInvokeExplicitlyImplementedMethod()
        {
			var employee = EmployeeType.CreateInstance();
            var currentMeters = (double) employee.GetFieldValue( "metersTravelled" );
            employee.Invoke( Flags.InstanceCriteria | Flags.TrimExplicitlyImplemented, "Swim", 100d );
            VerifyFields( employee, new { metersTravelled = currentMeters + 100 } );
        }

        [TestMethod]
        public void TestInvokeBaseClassMethods()
        {
            var employee = EmployeeType.CreateInstance();
            var currentMeters = (double) employee.GetFieldValue( "metersTravelled" );
            employee.Invoke( "Walk", 100d );
            VerifyFields( employee, new { metersTravelled = currentMeters + 100 } );
        }

        [TestMethod]
        public void TestInvokeStaticMethod()
        {
            RunWith( ( Type type ) =>
               {
                   var totalPeopleCreated = (int) type.GetFieldValue( "totalPeopleCreated" );
                   Assert.AreEqual( totalPeopleCreated, type.Invoke( "GetTotalPeopleCreated" ) );
               } );
        }

        [TestMethod]
        public void TestInvokeStaticMethodViaMethodInfo()
        {
            RunWith( ( Type type ) =>
               {
                   var totalPeopleCreated = (int) type.GetFieldValue( "totalPeopleCreated" );
                   Assert.AreEqual( totalPeopleCreated,
                                    type.Method( "GetTotalPeopleCreated", Flags.StaticCriteria ).Invoke() );
               } );
        }

        [TestMethod]
        public void TestInvokeStaticMethodsWithArgument()
        {
            RunWith( ( Type type ) =>
               {
                   var totalPeopleCreated = (int) type.GetFieldValue( "totalPeopleCreated" );
                   Assert.AreEqual( totalPeopleCreated + 20, type.Invoke( "AdjustTotalPeopleCreated", 20 ) );
               } );
        }

        [TestMethod]
        [ExpectedException( typeof(MissingMethodException) )]
        public void TestInvokeNonExistentInstanceMethod()
        {
            RunWith( ( object person ) => person.Invoke( "not_exist" ) );
        }

        [TestMethod]
        [ExpectedException( typeof(MissingMethodException) )]
        public void TestInvokeNonExistentStaticMethod()
        {
            RunWith( ( Type type ) => type.Invoke( "not_exist" ) );
        }
    }
}