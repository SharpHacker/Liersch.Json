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
using System.Text;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  partial class SLJsonNode
  {
    public string AsJson { get { return Serialize(true); } }
    public string AsJsonCompact { get { return Serialize(false); } }

    //------------------------------------------------------------------------

    public string Serialize(bool indented)
    {
      SLJsonWriter wr=new SLJsonWriter(new StringBuilder(), indented);
      Serialize(wr);
      return wr.ToString();
    }

    public void Serialize(SLJsonWriter writer)
    {
      if(IsArray) SerializeArray(writer);
      else if(IsObject) SerializeObject(writer);
      else if(IsValue) SerializeValue(writer);
      else if(IsNull) writer.WriteValueNull();
      else throw new InvalidOperationException();
    }

    //------------------------------------------------------------------------

    void SerializeArray(SLJsonWriter writer)
    {
      writer.BeginArray();
      foreach(SLJsonNode n in this)
        n.Serialize(writer);
      writer.EndArray();
    }

    void SerializeObject(SLJsonWriter writer)
    {
      writer.BeginObject();

      foreach(string n in Names)
      {
        writer.BeginField(n);
        this[n].Serialize(writer);
      }

      writer.EndObject();
    }

    void SerializeValue(SLJsonWriter writer)
    {
      switch(NodeType)
      {
        case SLJsonNodeType.Null: writer.WriteValueNull(); break;
        case SLJsonNodeType.Boolean: writer.WriteValue(AsBoolean); break;
        case SLJsonNodeType.Number: writer.WriteValue(AsDouble); break;
        case SLJsonNodeType.String: writer.WriteValue(AsString); break;
        default: throw new InvalidOperationException();
      }
    }
  }

  //--------------------------------------------------------------------------
}