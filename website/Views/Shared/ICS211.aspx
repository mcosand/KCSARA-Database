<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ExpandedRowsContext>" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>ICS211</title>
    <style type="text/css">
        @page
        {
            size: landscape;
            margin: 0.5in;
        }
        body
        {
            font-size: 10pt;
            font-family: "Arial" , "sans-serif";
        }
        td
        {
            border: solid 1pt black;
            border-collapse: collapse;
        }
        .t1 tr td { vertical-align:top; text-align:left; }
        .t2 tr td { text-align: center; }
        th
        {
            border: solid 1pt black;
            border-collapse: collapse;
        }
        .pb{ page-break-before:always; }
        .t1
        {
            border-top: 3pt solid black;
        }
        .t3
        {
            border-bottom: 3pt solid black;
        }
        .b
        {
            font-weight: bold;
        }
        .l { text-align: left; }
        .r
        {
            text-align: right;
        }
        .c
        {
            text-align: center;
        }
        .g
        {
            background-color: #dddddd;
        }
        div
        {
            padding: 0 .08in;
        }
    </style>
</head>
<body>
  <%
      int pageCount = Model.Rows.Count / 13 + ((Model.Rows.Count % 13 == 0) ? 0 : 1);
      Dictionary<Guid, string> numbers = (Dictionary<Guid, string>)ViewData["phones"];
     for (int i = 0; i < pageCount; i++)
     {
         string tableClass = "t1 pb";
         if (i == 0) tableClass = "t1";
         %>

    <table class="<%: tableClass %>" cellpadding="0" style="width: 10.33in">
        <tr>
            <td class="b r" style="font-size: 14pt; width: 1.97in; height: .47in">
                <div>
                    CHECK-IN LIST</div>
            </td>
            <td class="b" style="width: 3.42in">
                <div>1.Incident Name, DEM, KCSO #: <%: Model.SarEvent.Title%>, <%: Model.SarEvent.StateNumber %></div>
            </td>
            <td class="b" style="width: 2in">
                <div>2. Operational Period:</div>
            </td>
            <td style="width: 1.4in">
                Date From: <%: Model.SarEvent.StartTime.ToShortDateString()%><br />
                Time From: <%: Model.SarEvent.StartTime.ToShortTimeString()%>
            </td>
            <td style="width: 1.4in">
                Date To: <%: Model.SarEvent.StopTime.HasValue ? Model.SarEvent.StopTime.Value.ToShortDateString() : ""%><br />
                Time To: <%: Model.SarEvent.StopTime.HasValue ? Model.SarEvent.StopTime.Value.ToShortTimeString() : ""%>
            </td>
        </tr>
    </table>
    <table cellpadding="0" class="t2" style="width: 10.33in">
        <tr>
            <td colspan="4" style="text-align:left;">
                <div>3. Team/Unit Name:</div>
            </td>
            <td colspan="7" style="text-align:left;">
                <div>4. Check-In Location: <%: Model.SarEvent.Location%></div>
            </td>
        </tr>
        <tr>
            <td style="font-weight:bold; width: 0.63in; height:.18in">
                5.
            </td>
            <td style="font-weight:bold; width: .61in">
                6.
            </td>
            <td style="font-weight:bold; width: 2.25in">
                7.
            </td>
            <td style="font-weight:bold; width: 1.7in">
                8.
            </td>
            <td style="font-weight:bold; width: .78in">
                9.
            </td>
            <td style="font-weight:bold; width: .88in">
                10.
            </td>
            <td style="font-weight:bold; width: 0.75in">
                11.
            </td>
            <td style="font-weight:bold; width: 0.61in">
                12.
            </td>
            <td style="font-weight:bold; width: 0.68in">
                13.
            </td>
            <td style="font-weight:bold; width: 0.68in">
                14.
            </td>
            <td style="font-weight:bold; width: 0.54in">
                15.
            </td>
        </tr>
        <tr>
            <th style=" height:.79in">
                DEM #
            </th>
            <th>
                UNIT
            </th>
            <th>
                PRINT NAME
            </th>
            <th>
                CELL PHONE #
            </th>
            <th>
                Time In
            </th>
            <th>
                MUST BE OUT BY
            </th>
            <th>
                Time Out
            </th>
            <th>
                Over 100 Miles
            </th>
            <th class="g">
                Left Home
            </th>
            <th class="g">
                Arrive Home
            </th>
            <th class="g">
                Total Miles
            </th>
        </tr>

        <%
            var source = (Model.SarEvent is Mission)
                ? Model.Rows.OrderBy(f => ((MissionRoster)f).Unit.DisplayName).ThenBy(f => f.Person.ReverseName).Skip(i * 13).Take(13) 
                : Model.Rows.OrderBy(f => f.Person.ReverseName).Skip(i * 13).Take(13);
            foreach (var row in source)
           { %>
           <tr><td style="height:.35in"><%: row.Person.DEM%></td><td><%: (Model.SarEvent is Mission) ? ((MissionRoster)row).Unit.DisplayName : "" %></td><td style="text-align:left;"><div><%: row.Person.ReverseName%></div></td>
           <td><%: numbers[row.Person.Id] %></td>
           <td></td><td></td><td></td><td><%: (row.Miles >= 100) ? "X" : ""%></td>
           <td class="g"><%: Html.RelativeDateTimeString(row.TimeIn, Model.SarEvent.StartTime) %></td>
           <td class="g"><%: Html.RelativeDateTimeString(row.TimeOut, Model.SarEvent.StartTime) %></td>
           <td class="g"><%: row.Miles%></td>
           </tr>
        <% } %>
        <tr>
            <td colspan="11" style="text-align:left; height:.29in">
                <div>PREPARED BY KCSARA Database, </div>
            </td>
        </tr>
    </table>
    <table class="t3" cellpadding="0" style="width: 10.33in">
        <tr>
            <td style="width: 3.63in; height:.26in;">
                <div>ICS 111-SAR &nbsp; &nbsp; <%: i + 1 %> of <%: pageCount %></div>
            </td>
            <td><div>Date/Time: <%: DateTime.Now %></div></td>
        </tr>
    </table>
    <% } %>
</body>
</html>