﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using SolTechnology.Avro;
using Xunit;

namespace AvroConvertComponentTests.DefaultSerializationDeserialization
{
    public class AvroAttributeClassTests
    {
        private readonly Fixture _fixture;

        public AvroAttributeClassTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Component_ClassContainsAvroAttributes_AttributeValuesAreResolved()
        {
            //Arrange
            AttributeClass toSerialize = _fixture.Create<AttributeClass>();


            //Act
            var result = AvroConvert.Serialize(toSerialize);

            var deserialized = AvroConvert.Deserialize<User>(result);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(deserialized);
            Assert.Equal(toSerialize.NullableIntProperty, deserialized.favorite_number);
            Assert.Equal(toSerialize.NullableStringProperty, deserialized.name);
            Assert.Equal(toSerialize.AndAnotherString, deserialized.favorite_color);
        }

        [Fact]
        public void Component_FieldClassContainsAvroAttributes_AttributeValuesAreResolved()
        {
            //Arrange
            AttributeClassWithoutGetters toSerialize = _fixture.Create<AttributeClassWithoutGetters>();


            //Act
            var result = AvroConvert.Serialize(toSerialize);

            var deserialized = AvroConvert.Deserialize<User>(result);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(deserialized);
            Assert.Equal(toSerialize.NullableIntProperty, deserialized.favorite_number);
            Assert.Equal(toSerialize.StringProperty, deserialized.name);
            Assert.Equal(toSerialize.AndAnotherString, deserialized.favorite_color);
        }

        [Fact]
        public void Component_ComplexStruct_AttributeValuesAreResolved()
        {
            //Arrange
            ComplexStruct toSerialize = new ComplexStruct(_fixture.Create<List<int>>());


            //Act
            var result = AvroConvert.Serialize(toSerialize);

            var deserialized = AvroConvert.Deserialize<ComplexStruct>(result);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(toSerialize, deserialized);
        }

        [Fact]
        public void Component_WriteSchemaContainsDefaultValuesForProperies_NullValuesAreReplacedWithDefault()
        {
            //Arrange
            DefaultValueClass defaultValueClass = new DefaultValueClass();
            defaultValueClass.andLongBigDefaultedProperty = null;
            defaultValueClass.justSomeProperty = null;
            defaultValueClass.andLongSmallDefaultedProperty = null;

            //Act
            var result = AvroConvert.Serialize(defaultValueClass);

            var deserialized = AvroConvert.Deserialize<DefaultValueClass>(result);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(deserialized);
            Assert.Equal("Let's go", deserialized.justSomeProperty);
            Assert.Equal(9200000000000000007, deserialized.andLongBigDefaultedProperty);
            Assert.Equal(100, deserialized.andLongSmallDefaultedProperty);
            Assert.Null(deserialized.andNullProperty);
        }

        [Fact]
        public void Component_ClassContainsAvroAttributes_IgnoredPropertiesAreNotSerialized()
        {
            //Arrange
            AttributeClass toSerialize = _fixture.Create<AttributeClass>();

            //Act
            var result = AvroConvert.Serialize(toSerialize);

            var deserialized = AvroConvert.Deserialize<AttributeClass>(result);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(deserialized);
            Assert.Null(deserialized.IgnoredProperty);
        }

        [Fact(Skip = "Scenario not supported")]
        public void Component_PrivatePropertyIsMarkedAsDataMember_ItIsSerialized()
        {
            //Arrange
            long expectedValue = 2137;

            AttributeClass toSerialize = _fixture.Create<AttributeClass>();
            typeof(AttributeClass)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.Name.Contains("privateProperty"))?
                .SetValue(toSerialize, expectedValue);


            //Act
            var result = AvroConvert.Serialize(toSerialize);

            var deserialized = AvroConvert.Deserialize<AttributeClass>(result);


            //Assert
            Assert.NotNull(result);
            Assert.NotNull(deserialized);

            var resultValue = typeof(AttributeClass)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.Name.Contains("privateProperty"))?
                .GetValue(deserialized);

            Assert.Equal(expectedValue, resultValue);
        }
    }
}
