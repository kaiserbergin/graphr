using System;
using System.Collections.Generic;
using Graphr.Neo4j.Attributes;
using Neo4j.Driver;

namespace Graphr.Tests.Graphr.Models
{
    [NeoNode("ValueTypesNode")]
    public class ValueTypesNode
    {
        [NeoProperty("localDateTime")] 
        public LocalDateTime LocalDateTime { get; set; }
        [NeoProperty("dateTimeForZonedDateTime")]
        public ZonedDateTime ZonedDateTimeFromDateTime { get; set; }
        [NeoProperty("string")]
        public string String { get; set; }
        [NeoProperty("intList")]
        public List<int> IntList { get; set; }
        [NeoProperty("float")]
        public double FloatButItsADouble { get; set; }
        [NeoProperty("point")]
        public Point Point { get; set; }
        [NeoProperty("int")]
        public long IntButItsALong { get; set; }
        [NeoProperty("duration")]
        public Duration Duration { get; set; }
        [NeoProperty("localTime")]
        public LocalTime LocalTime { get; set; }
        [NeoProperty("localTimeForDateTime")]
        public DateTime DateTimeFromLocalTime { get; set; }
        [NeoProperty("dateForDateTime")]
        public DateTime DateTimeFromDate { get; set; }
        [NeoProperty("boolean")]
        public bool Bool { get; set; }
        [NeoProperty("dateTimeForDateTimeOffset")]
        public DateTimeOffset DateTimeOffsetFromDateTime { get; set; }
        [NeoProperty("localTimeForTimeSpan")]
        public TimeSpan TimeSpanFromLocalTime { get; set; }
        [NeoProperty("localDateTimeForDateTime")]
        public DateTime DateTimeFromLocalDateTime { get; set; }
        [NeoProperty("time")]
        public OffsetTime OffsetTime { get; set; }
        [NeoProperty("dateForLocalDate")]
        public LocalDate LocalDateFromDate { get; set; }
        [NeoProperty("timestamp")]
        public long Timestamp { get; set; }
        [NeoProperty("floatForString")]
        public string FloatButItsAString { get; set; }
    }
}