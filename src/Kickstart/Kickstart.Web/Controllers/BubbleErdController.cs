using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Kickstart.Services.Types;
using Microsoft.AspNetCore.Mvc;

namespace Kickstart.Web.Controllers
{
    [Route("api/[controller]")]
    public class BubbleErdController : Controller
    {

        [HttpGet("[action]")]
        public d3Graph BubbleGraphData()
        {
            var channel = new Channel("localhost:50083", ChannelCredentials.Insecure);

            var client = new KickstartServiceApi.KickstartServiceApiClient(channel);

            var response = client.QueryDatabaseTables(new QueryDatabaseTablesRequest() { Schema="sp"});

            var graphData = new d3Graph();

            graphData.Nodes= response.Tables.Select(t => new d3Node() { Id=$"{t.SchemaName.ToLower()}_{t.TableName.ToLower()}" , Label=t.TableName, Group =1 }).ToList();

            var links = new List<d3Link>();
            foreach (var table in response.Tables)
            {
                foreach (var column in table.Columns)
                {
                    foreach (var fk in column.ForeignKeys)
                    {
                        var fkTable = response.Tables.FirstOrDefault(t => t.SchemaName.ToLower() == fk.SchemaName.ToLower() &&  t.TableName.ToLower() == fk.TableName.ToLower());
                        if (fkTable !=null)
                        {
                            var node = graphData.Nodes.FirstOrDefault(n=>n.Id == $"{fkTable.SchemaName.ToLower()}_{fkTable.TableName.ToLower()}");
                            if (node != null)
                            {
                                node.NumberOfChildren++;
                            }

                            links.Add(new d3Link() {Source =  $"{table.SchemaName.ToLower()}_{table.TableName.ToLower()}", Target = $"{fk.SchemaName.ToLower()}_{fk.TableName.ToLower()}", Value =5});
                        }
                        else
                        {
                            //skip it, not part of diagram
                        }
                    }
                }
            }

            graphData.Links = links;
            return graphData;
        }
        
    }

    public class d3Graph
    {
        public IEnumerable<d3Node> Nodes { get; set; }
        public IEnumerable<d3Link> Links { get; set; }

    }
    public class d3Node
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public int Group { get; set; }
        public int NumberOfChildren { get; set; }
    }

    public class d3Link
    {
        public string Source { get; set; }
        public string Target { get; set; }

        public int Value { get; set; }
    }
}
