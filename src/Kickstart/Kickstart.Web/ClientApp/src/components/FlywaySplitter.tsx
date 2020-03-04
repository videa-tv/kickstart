import '../css/form-labels-on-top.css';
import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import 'isomorphic-fetch';
import { render } from 'react-dom';
import * as CopyToClipboard from 'react-copy-to-clipboard'
//import samplesql from  '../Sample/Sql/AR_Tables.sql';
//import * as JsZip from 'jszip'
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import 'react-tabs/style/react-tabs.css';
interface FlywaySplitterState {
    copied: boolean;
    loading: boolean;
    splitting: boolean;
    databaseType: number;
    unSplitTableDDL: string;
    unSplitTableTypeDDL: string;
    unSplitViewDDL: string;
    unSplitFunctionDDL: string;
    unSplitStoredProcedureDDL: string;
    split: Split;
}

interface FlywaySplitterModel {
    CreateDatabaseProject: boolean
}

interface Split {
    zipAsBase64: string;
}


export class FlywaySplitter extends React.Component<RouteComponentProps<{}>, FlywaySplitterState> {
    constructor(props: any) {
        super(props);
        this.state = {
            copied: false, loading: false, splitting: false, databaseType: 1, unSplitTableDDL: "", unSplitTableTypeDDL: "",
            unSplitViewDDL: "",
            unSplitFunctionDDL:"",
            unSplitStoredProcedureDDL: "",
            split: { zipAsBase64: "" }
        }; 
       
        
    }

    public render() {
        
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderForm();

        return <div>{ contents }</div>;
    }
    
    public loadSampleStoredProcs(samplesqlStoredProcPath: string) {

        fetch(samplesqlStoredProcPath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unSplitStoredProcedureDDL + text;
                this.setState({ unSplitStoredProcedureDDL: text });
            });
    }
    public loadSampleTables(samplesqlPath : string)
    {
        fetch(samplesqlPath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unSplitTableDDL + text;
                this.setState({ unSplitTableDDL: text });
            });

    }

    public loadSampleTableTypes(samplesqlTableTypePath: string) {
        fetch(samplesqlTableTypePath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unSplitTableTypeDDL + text;
                this.setState({ unSplitTableTypeDDL: text });
            });

    }

    public loadSampleViews(samplesqlViewPath: string) {
        fetch(samplesqlViewPath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unSplitViewDDL + text;
                this.setState({ unSplitViewDDL: text });
            });

    }

    public loadSampleFunctions(samplesqlFunctionsPath: string) {
        fetch(samplesqlFunctionsPath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unSplitFunctionDDL + text;
                this.setState({ unSplitFunctionDDL: text });
            });

    }

    public loadSampleSqlServer() {


        this.setState({
            databaseType: 2
        });

        this.loadSampleTables(require("../Sample/Sql/Sample1/Tables.sql"));
        
        this.loadSampleTableTypes(require("../Sample/Sql/Sample1/TableTypes.sql"));
        this.loadSampleViews(require("../Sample/Sql/Sample1/Views.sql"));
        this.loadSampleFunctions(require("../Sample/Sql/Sample1/Functions.sql"));

        this.loadSampleStoredProcs(require("../Sample/Sql/Sample1/StoredProcs.sql"));

    }

    public loadSamplePostgres() {

        
        this.loadSampleStoredProcs(require("../Sample/Sql/Postgresql.sql"));
    }

    
    handleChange1(e: any ) {
        var val: string = e.target.value;
        
        this.setState({ unSplitTableDDL:val });
        //this.setState({ [e.target.id]: e.target.value });

    }
    handleChange2(e: any) {
        var val: string = e.target.value;

        this.setState({ unSplitTableTypeDDL: val });
        //this.setState({ [e.target.id]: e.target.value });

    }
    handleChange3(e: any) {
        var val: string = e.target.value;

        this.setState({ unSplitViewDDL: val });
        //this.setState({ [e.target.id]: e.target.value });

    }
    handleChange4(e: any) {
        var val: string = e.target.value;

        this.setState({ unSplitFunctionDDL: val });
        //this.setState({ [e.target.id]: e.target.value });

    }

    handleChange5(e: any) {
        var val: string = e.target.value;

        this.setState({ unSplitStoredProcedureDDL: val });
        //this.setState({ [e.target.id]: e.target.value });

    }

    onCopy() {
        this.setState({ copied: true });
    }
    public reset() {
        
        this.setState({ unSplitTableDDL: "" });
        this.setState({ unSplitTableTypeDDL: "" });
        this.setState({ unSplitViewDDL: "" });
        this.setState({ unSplitFunctionDDL: "" });
        this.setState({ unSplitStoredProcedureDDL: "" });

        let j = this.state.split;
        //j.convertedTableDDL = "";
        //this.setState({ converted: j });
    }

    public downloadFiles() {
        this.download("Flyway.zip",this.state.split.zipAsBase64 );
       }
    public download(filename: string, text: string) {
        var textval: any = text;
        
        var byteCharacters = atob(textval);
        
        var byteNumbers = new Array(byteCharacters.length);
        for (var i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }

        var byteArray = new Uint8Array(byteNumbers);
        var blob = new Blob([byteArray], { type: "octet/stream" });

        
        var element = document.createElement('a');
        var url = window.URL.createObjectURL(blob);

        //element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        element.href = url;
        element.setAttribute('download', filename);

        element.style.display = 'none';
        document.body.appendChild(element);

        element.click();

        document.body.removeChild(element);
    }
    
    public handleClick(e: React.MouseEvent<HTMLButtonElement>) {
        e.preventDefault();
        this.setState({ splitting: true });

        this.postData('api/FlywaySplitter/Split', {
            databaseType: this.state.databaseType,
            unSplitTableDDL: this.state.unSplitTableDDL,
            unSplitTableTypeDDL: this.state.unSplitTableTypeDDL,
            unSplitViewDDL: this.state.unSplitViewDDL,
            unSplitFunctionDDL: this.state.unSplitFunctionDDL,
            unSplitStoredProcedureDDL: this.state.unSplitStoredProcedureDDL
        });
      

    }
    public postData(url : string, data: object) {
        fetch(url,
            {
                headers: {
                    'content-type': 'application/json'
                },
                method: "post",
                body: JSON.stringify(data)
            }
        )
            .then(response =>
                response.json() as Promise<Split>
            )
            .then((data) => {
                let c = this.state.split;
                c.zipAsBase64 = data.zipAsBase64;
                this.setState({ split: c });
                this.setState({ splitting: false });
            });
};
    handleDbmsChange(event: any) {
        let value = event.target.value;
   
        this.setState({
            databaseType: value
        });
    }

    private renderForm() {
        let kickstartModel = {} as FlywaySplitterModel;
        kickstartModel.CreateDatabaseProject = false;

        var convertbutton;
        if (this.state.splitting) {
            convertbutton = <button type="button" value="Splitting" onClick={e => this.handleClick(e)}>Splitting</button>
        }
        else {
            convertbutton = <button type="button" value="Split" onClick={e => this.handleClick(e)}>Split</button>
        }

        var results;

        if (this.state.split.zipAsBase64.length > 0) {
            results =
                <div>
                <button onClick={e=> this.downloadFiles()} >Download Flyway Files</button>
                </div>
        }

        return (
            <form className="form-labels-on-top">
                <div className="form-title-row">
                    <h1>Flyway Splitter</h1>
                </div>

                <div className="form-labels-on-top">
                    <div className="form-row">
                        <label><span>Target Data Store</span></label>
                        <div>
                            <label>
                                <select value={this.state.databaseType}  onChange={(e) => this.handleDbmsChange(e)}>
                                    <option value="1">Postgresql 9.6</option>
                                    <option value="2">Sql Server 2014</option>
                                </select>
                            </label>
                        </div>
                    </div>

                    <div className="form-row">
                        <label><span>SQL DDL</span></label>
                        <div>
                            <Tabs>
                                <TabList>
                                    <Tab>Tables</Tab>
                                    <Tab>Table Types</Tab>
                                    <Tab>Views</Tab>
                                    <Tab>Functions</Tab>
                                    <Tab>Stored Procs</Tab>
                                </TabList>

                                <TabPanel>
                                    <label>
                                        <textarea name="TextAreaunSplitTableDDL" value={this.state.unSplitTableDDL} onChange={this.handleChange1.bind(this)}   ></textarea>

                                    </label>
                                    <button type="button" onClick={e => this.loadSamplePostgres()}> Load Sample (Postgres) </button>
                                    <button type="button" onClick={e => this.loadSampleSqlServer()}> Load Sample (Sql Server) </button>


                                </TabPanel>
                                <TabPanel>
                                    <label>
                                        <textarea name="TextunSplitTableTypeDDL" value={this.state.unSplitTableTypeDDL} onChange={this.handleChange2.bind(this)}  ></textarea>

                                    </label>
                                    
                                </TabPanel>
                                <TabPanel>
                                    <label>
                                        <textarea name="TextunSplitViewDDL" value={this.state.unSplitViewDDL} onChange={this.handleChange3.bind(this)}  ></textarea>

                                    </label>

                                </TabPanel>
                                <TabPanel>
                                    <label>
                                        <textarea name="TextunSplitFunctionDDL" value={this.state.unSplitFunctionDDL} onChange={this.handleChange4.bind(this)}  ></textarea>

                                    </label>

                                </TabPanel>
                                <TabPanel>
                                    <label>
                                        <textarea name="unSplitStoredProcedureDDL" value={this.state.unSplitStoredProcedureDDL} onChange={this.handleChange5.bind(this)}  ></textarea>

                                    </label>
                                    
                                </TabPanel>
                            </Tabs>
                          
                        </div>
                    </div>

                    <div className="form-row">

                        {convertbutton}
                        <button type="button" value="Reset" onClick={ e=>  this.reset() }>Reset</button>
                    </div>
                   
                </div>
                <div className="labels-on-top">

                        {results}
                </div>
                    </form>
    
        );
    }
}



