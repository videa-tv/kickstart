import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { RouteComponentProps } from 'react-router';
import BubbleErdApp from './BubbleErdApp/BubbleErdApp';
import './BubbleErdApp/index.css';
import data from "./BubbleErdApp/miserables";
import { d3Types } from "./BubbleErdApp/types";

interface BubbleErdState {
    d3Graph: d3Types.d3Graph;
}

export class BubbleErd extends React.Component<RouteComponentProps<{}>, BubbleErdState> {
    constructor(props: any) {
        super(props);
        this.state = {
            d3Graph: {
                nodes: [],
                links: []
        }};
        

      
        
    }

    private fetchNodes() {
        fetch('api/BubbleErd/BubbleGraphData')
            .then(response => response.json() as Promise<d3Types.d3Graph>)
            .then(data => {

                let c = this.state.d3Graph;
                c.nodes = data.nodes;
                c.links = data.links;
                this.setState({ d3Graph: c });
                
                
            });
     
    }
    /*
    private fetchLinks() {
        fetch('api/BubbleErd/BubbleLinks')
            .then(response => response.json() as Promise<d3Types.d3Link[]>)
            .then(data => {

                let c = this.state.d3Graph;
                c.links = data;
                this.setState({ d3Graph: c });

            });

    }*/

    public render() {

        if (/*this.state.d3Graph.links.length === 0 ||*/this.state.d3Graph.nodes === null || this.state.d3Graph.nodes.length === 0) {
            this.fetchNodes();
            //this.fetchLinks();
            return <div>Loading</div>;
        }
       
        let contents = this.renderGraph();

        return <div>

                   {contents}
               </div>;

    }

    private renderGraph() {

        //alert(JSON.stringify(this.state.d3Graph));
            return (
                <BubbleErdApp
                    width={window.screen.availWidth}
                    height={window.screen.availHeight}
                    graph={this.state.d3Graph}/>
            );
    }

    
}


