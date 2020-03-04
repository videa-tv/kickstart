import * as React from "react";
import * as d3 from "d3";
import { d3Types } from "./types";

class Node extends React.Component<{ node: d3Types.d3Node, color: string }, {}> {
  ref: SVGCircleElement;

  componentDidMount() {
    d3.select(this.ref).data([this.props.node]);
  }

  render() {
    return (
        <g>
        <circle className="node" r={20 + (this.props.node.numberOfChildren *3)} fill={this.props.color}
        ref={(ref: SVGCircleElement) => this.ref = ref}>
        <title>{this.props.node.id}</title>
            </circle>
            
            </g>
    );
  }
}

export default class Nodes extends React.Component<{ nodes: d3Types.d3Node[], simulation: any }, {}> {
  componentDidMount() {
    const simulation = this.props.simulation;
    d3.selectAll(".node")
      .call(d3.drag()
        .on("start", onDragStart)
        .on("drag", onDrag)
        .on("end", onDragEnd));

    function onDragStart(d: any) {
      if (!d3.event.active) {
        simulation.alphaTarget(0.3).restart();
      }
      d.fx = d.x;
      d.fy = d.y;
    }

    function onDrag(d: any) {
      d.fx = d3.event.x;
      d.fy = d3.event.y;
    }

      var self = this;

      function onDragEnd(d: any) {
      if (!d3.event.active) {
       // simulation.alphaTarget(0);
      }
      //d.fx = null;
        //d.fy = null;
        var svg = document.getElementsByTagName("svg")[0];
        var svg_xml = (new XMLSerializer).serializeToString(svg);
          //add name spaces.
          if (!svg_xml.match(/^<svg[^>]+xmlns="http\:\/\/www\.w3\.org\/2000\/svg"/)) {
              svg_xml = svg_xml.replace(/^<svg/, '<svg xmlns="http://www.w3.org/2000/svg"');
          }
          if (!svg_xml.match(/^<svg[^>]+"http\:\/\/www\.w3\.org\/1999\/xlink"/)) {
              svg_xml = svg_xml.replace(/^<svg/, '<svg xmlns:xlink="http://www.w3.org/1999/xlink"');
          }

          //add xml declaration
          svg_xml = '<?xml version="1.0" standalone="no"?>\r\n' + svg_xml;

        self.download("bob.svg", svg_xml);
     
    }
  }

    public download(filename: string, text: string) {
        var textval: any = text;
        
        var element = document.createElement('a');
        element.innerHTML = "Download Svg";
        var div = document.getElementById("maindiv");
        if (div != null) {
            div.appendChild(element);
        }
        var blob;
        try {


             blob = new Blob(
                 [textval ],
                {
                    type: "text/plain;charset=utf-8"
                }
            );
        }
        catch (err) {
            alert(err.message);
        }
        var url = window.URL.createObjectURL(blob);
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        //element.href = url;
        element.setAttribute('download', filename);

        //element.style.display = 'none';
        
        //element.click();

        //document.body.removeChild(element);
    }

  render() {
    const color = d3.scaleOrdinal(d3.schemeCategory10);
    const nodes = this.props.nodes.map((node: d3Types.d3Node, index: number) => {
      return <Node  key={index} node={node} color={color(node.group.toString())} />;
    });

    return (
      <g className="nodes">
        {nodes}
      </g>
    );
  }
}
