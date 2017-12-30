﻿//----------------------------------------------------------------------------
//
// Copyright © 2013-2017 Dipl.-Ing. (BA) Steffen Liersch
// All rights reserved.
//
// Steffen Liersch
// Robert-Schumann-Straße 1
// 08289 Schneeberg
// Germany
//
// Phone: +49-3772-38 28 08
// E-Mail: S.Liersch@gmx.de
//
//----------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  sealed class UnitTest1
  {
    public void Run()
    {
      Test=new UnitTestHelper();
      Test.PrintHeadline("UnitTest1 - Basic Features");

      TestReadOnly();
      TestReadWrite();
      TestCreateNew();
      TestOperators();
      TestSerialization();

      Test.PrintSummary();
      Test=null;
    }

    void TestReadOnly()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);

      Test.Assert(() => n["person"]["firstName"].AsString=="Jane");
      Test.Assert(() => n["person"]["lastName"].AsString=="Doe");
      Test.Assert(() => n["person"]["zipCode"].AsString=="12345");

      Test.Assert(() => !n["person"]["street"].IsReadOnly);
      SLJsonMonitor m=n.CreateMonitor();
      Test.Assert(() => !m.IsModified);
      Test.Assert(() => !m.IsReadOnly);
      m.IsReadOnly=true;
      Test.Assert(() => n["person"]["street"].IsReadOnly);

      Test.Assert(() => n["test"]["emptyArray"].IsArray);
      Test.Assert(() => n["test"]["emptyArray"].Count==0);
      Test.Assert(() => n["test"]["emptyObject"].IsObject);
      Test.Assert(() => n["test"]["emptyObject"].Count==0);

      Test.Assert(() => n["test"]["testArray"].IsArray);
      Test.Assert(() => n["test"]["testArray"].Count==4);
      Test.Assert(() => n["test"]["testArray"][0].AsInt32==10);
      Test.Assert(() => n["test"]["testArray"][1].AsInt32==20);
      Test.Assert(() => n["test"]["testArray"][2].AsInt32==30);
      Test.Assert(() => n["test"]["testArray"][3].AsInt32==40);
      Test.Assert(() => n["test"]["testArray"][4].AsInt32==0); // Access to missing entry
      Test.Assert(() => !n["test"]["testArray"][4].IsValue); // Check missing entry
      Test.Assert(() => n["test"]["testArray"].Count==4); // Check count again

      Test.Assert(() => n["test"]["testObject"].NodeType==SLJsonNodeType.Object);
      Test.Assert(() => n["test"]["testObject"].Count==2);
      Test.Assert(() => n["test"]["testObject"].AsString==null);

      Test.Assert(() => n["test"]["testValueMissing__"].NodeType==SLJsonNodeType.Missing);
      Test.Assert(() => !n["test"]["testValueMissing__"].AsBoolean);
      Test.Assert(() => n["test"]["testValueMissing__"].AsInt32==0);
      Test.Assert(() => n["test"]["testValueMissing__"].AsString==null);

      Test.Assert(() => n["test"]["testValueNull"].NodeType==SLJsonNodeType.Null);
      Test.Assert(() => !n["test"]["testValueNull"].AsBoolean);
      Test.Assert(() => n["test"]["testValueNull"].AsInt32==0);
      Test.Assert(() => n["test"]["testValueNull"].AsString==null);

      Test.Assert(() => n["test"]["testValueTrue"].NodeType==SLJsonNodeType.Boolean);
      Test.Assert(() => n["test"]["testValueTrue"].AsBoolean);
      Test.Assert(() => n["test"]["testValueTrue"].AsInt32==0);
      Test.Assert(() => n["test"]["testValueTrue"].AsString=="true");

      Test.Assert(() => n["test"]["testValue32"].NodeType==SLJsonNodeType.Number);
      Test.Assert(() => n["test"]["testValue32"].AsInt32==256);
      Test.Assert(() => n["test"]["testValue32"].AsString=="+256");

      Test.Assert(() => !m.IsModified);
      Test.Assert(() => m.IsReadOnly);
    }

    void TestReadWrite()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);
      SLJsonMonitor m=n.CreateMonitor();

      // Try to read some properties
      Test.Assert(() => n["person"]["firstName"].AsString=="Jane");
      Test.Assert(() => n["person"]["lastName"].AsString=="Doe");
      Test.Assert(() => n["person"]["zipCode"].AsString=="12345");
      Test.Assert(() => !m.IsModified);

      try
      {
        m.IsReadOnly=true;
        n["abc"]["def"][100]=0;
        Test.Assert(() => false);
      }
      catch(InvalidOperationException)
      {
        m.IsReadOnly=false;
        Test.Assert(() => true);
      }

      Test.Assert(() => !m.IsModified);

      // Try to change an existing property
      n["person"]["firstName"].AsString="John";
      Test.Assert(() => n["person"]["firstName"].AsString=="John");
      Test.Assert(() => m.IsModified);

      // Try to add a new property
      int c=n["person"].Count;
      Test.Assert(() => n["person"]["newProperty"].NodeType==SLJsonNodeType.Missing);
      n["person"]["newProperty"].AsInt32=333;
      Test.Assert(() => n["person"].Count==c+1);
      Test.Assert(() => n["person"]["newProperty"].NodeType==SLJsonNodeType.Number);
      Test.Assert(() => n["person"]["newProperty"].AsString=="333");

      // Try to delete a property
      c=n["person"].Count;
      Test.Assert(() => n["person"]["lastName"].NodeType==SLJsonNodeType.String);
      n["person"]["lastName"].Remove();
      Test.Assert(() => n["person"].Count==c-1);
      Test.Assert(() => n["person"]["lastName"].NodeType==SLJsonNodeType.Missing);
      Test.Assert(() => n["person"]["lastName"].AsString==null);
    }

    void TestCreateNew()
    {
      SLJsonNode n=new SLJsonNode();
      n["person"]["firstName"].AsString="John";
      n["person"]["lastName"].AsString="Doe";
      Test.Assert(() => n.Count==1);
      Test.Assert(() => n["person"].Count==2);
      Test.Assert(() => n["person"]["firstName"].AsString=="John");
      Test.Assert(() => n["person"]["lastName"].AsString=="Doe");

      n["intValue"].AsInt32=27;
      Test.Assert(() => n["intValue"].IsNumber);
      Test.Assert(() => n["intValue"].AsString=="27");
      Test.Assert(() => n["intValue"].Remove());
      Test.Assert(() => !n["intValue"].Remove());
      Test.Assert(() => n["intValue"].IsMissing);
      Test.Assert(() => n["intValue"].AsString==null);

      Test.Assert(() => n["testArray"].IsMissing);
      n["testArray"][0].AsInt32=11;
      Test.Assert(() => n["testArray"].IsArray);
      Test.Assert(() => n["testArray"].Count==1);
      n["testArray"][0].AsInt32=77;
      Test.Assert(() => n["testArray"].Count==1);
      Test.Assert(() => n["testArray"][0].AsInt32==77);
      Test.Assert(() => n["testArray"][1].IsMissing);
      n["testArray"][2].AsInt32=200;
      Test.Assert(() => n["testArray"][1].IsNull);
      Test.Assert(() => n["testArray"][2].IsNumber);
      Test.Assert(() => n["testArray"][3].IsMissing);
      n["testArray"][3].AsInt32=300;
      Test.Assert(() => n["testArray"][3].IsNumber);

      Test.Assert(() => n["testArray"].Count==4);
      Test.Assert(() => !n["testArray"][100].Remove());
      Test.Assert(() => !n["testArray"][100].Remove());
      Test.Assert(() => n["testArray"].Count==4);
      Test.Assert(() => n["testArray"][1].Remove());
      Test.Assert(() => n["testArray"].Count==3);
    }

    void TestOperators()
    {
      SLJsonNode n=new SLJsonNode();

      n["person"]["age"]=27;
      Test.Assert(() => n["person"]["age"].IsNumber);
      Test.Assert(() => n["person"]["age"]==27);
      Test.Assert(() => n["person"]["age"]=="27");

      n["person"]["age"].AsInt32=28;
      Test.Assert(() => n["person"]["age"].IsNumber);
      Test.Assert(() => n["person"]["age"]==28);
      Test.Assert(() => n["person"]["age"]=="28");

      Test.Assert(() => n["person"]["age"].Remove());
      Test.Assert(() => n["person"]["age"]!=null);
      Test.Assert(() => n["person"]["age"].IsMissing);

      n["person"]["age"].AsString="29";
      Test.Assert(() => n["person"]["age"].IsString);
      Test.Assert(() => n["person"]["age"]==29);
      Test.Assert(() => n["person"]["age"]=="29");

      n["person"]["age"]="30";
      Test.Assert(() => n["person"]["age"].IsString);
      Test.Assert(() => n["person"]["age"]==30);
      Test.Assert(() => n["person"]["age"]=="30");

      n["person"]["age"]=null;
      Test.Assert(() => n["person"]["age"].IsNull);
      Test.Assert(() => !n["person"]["age"].IsString);

      n["person"]["age"]=(string)null;
      Test.Assert(() => n["person"]["age"].IsNull);
      Test.Assert(() => !n["person"]["age"].IsString);

      n["person"]["age"].AsString=null;
      Test.Assert(() => n["person"]["age"].IsNull);
      Test.Assert(() => !n["person"]["age"].IsString);

      n["person"]["other"]["property"]="test_1";
      SLJsonNode o=n["person"]["other"]["property"];
      Test.Assert(() => o.IsString);
      Test.Assert(() => o.AsString=="test_1");
      o.AsString="test_2";
      Test.Assert(() => n["person"]["other"]["property"].AsString=="test_2");

      n["person"]["other"][2]=300;
      n["person"]["other"][1]=200;
      n["person"]["other"][0]=100;

      Test.Assert(() => o.AsString=="test_2");
      o.AsString="test_3";
      Test.Assert(() => o.AsString=="test_3");

      Test.Assert(() => n["person"]["other"][0].AsInt32==100);
      Test.Assert(() => n["person"]["other"][1].AsInt32==200);
      Test.Assert(() => n["person"]["other"][2].AsInt32==300);
      Test.Assert(() => n["person"]["other"]["property"].AsString==null);
      Test.Assert(() => n["person"]["other"]["property"].IsMissing);
    }

    void TestSerialization()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);

      n["newProperty"]["value"].AsInt32=27;
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.Number);
      Test.Assert(() => n["newProperty"]["value"].AsJson=="27");
      Test.Assert(() => RemoveWhitespace(n["newProperty"].AsJson)=="{\"value\":27}");

      n["newProperty"]["value"][0].AsInt32=100;
      n["newProperty"]["value"][3].AsInt32=333;
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.Array);
      Test.Assert(() => n["newProperty"].AsJsonCompact=="{\"value\":[100,null,null,333]}");

      n["newProperty"]["value"].AsString="Text";
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.String);

      n["newProperty"]["value"].AsString=null;
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.Null);

      Test.Assert(() => n["newProperty"]["value"].Remove());
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.Missing);
    }

    string RemoveWhitespace(string value) { return m_RegexWhitespace.Replace(value, string.Empty); }

    static string RetrieveJsonExpression()
    {
      return @"
      {
        person: {
          firstName: 'Jane',
          lastName: 'Doe',
          street: 'Main',
          number: '123 c',
          city: 'Democity',
          zipCode: '12345'
        },
        test: {
          emptyArray: [],
          emptyObject: {},
          testArray: [10, 20, 30, 40],
          testObject: {a: 'First', b: 'Second'},
          testValueNull: null,
          testValueFalse: false,
          testValueTrue: true,
          testValue32: +256,
          testValue64: 10000000000000000,
          testValueDouble: 3.1415
        }
      }";
    }

    Regex m_RegexWhitespace=new Regex("\\s", RegexOptions.CultureInvariant);
    UnitTestHelper Test;
  }

  //--------------------------------------------------------------------------
}