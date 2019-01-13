using System;
using System.Linq;
using UnityEngine;

namespace Domain {
    public class MapNodeKey {

        public static KeyType GetTagType(string type) {
            type = type.Replace(":", "_");
            KeyType enumType;
            if(Enum.TryParse(type, true, out enumType)) {
                return enumType;
            }

            Debug.LogWarning("Node type " + type + " not found!");
            return KeyType.None;
        }
        
        public enum KeyType {
            None,
            Abandoned,
            Addr_City,
            Addr_Contact,
            Addr_Housenumber,
            Addr_Postcode,
            Addr_Street,
            Amenity,
            Bar,
            Bench,
            Barrier,
            Bicycle_Road,
            Bridge,
            Building,
            Busway,
            Cabins,
            Car_Wash,
            Cemetery,
            Club,
            Condo,
            Crop,
            Crossing,
            Crossing_Ref,
            Footway,
            Highway,
            Leisure,
            Man_Made,
            Maxspeed,
            Minor,
            Name,
            Name_Lt,
            Name_Ru,
            Natural,
            Office,
            Official_Name,
            Oneway,
            Operator,
            Power,
            Tower_Type,
            Traffic_Calming,
            Traffic_Signals,
            Railway,
            Ref,
            Service,
            Shop,
            Source_Maxspeed,
            Sport,
            Surface,
            Website
        }
    }
}