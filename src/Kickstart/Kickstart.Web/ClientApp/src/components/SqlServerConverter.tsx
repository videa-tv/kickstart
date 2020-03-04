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
interface SqlServerConverterState {
    copied: boolean;
    loading: boolean;
    converting: boolean;
    convertToDatabaseType: string;
    convertToSnakeCase: boolean;
    unconvertedTableDDL: string;
    unconvertedTableTypeDDL: string;
    unconvertedViewDDL: string;
    unconvertedFunctionDDL: string;

    unconvertedStoredProcedureDDL: string;
    converted: Converted;
}

interface SqlServerConverterModel {
    CreateDatabaseProject: boolean
}

interface Converted {
    convertedTableDDL: string;
    convertedTableTypeDDL: string;
    convertedViewDDL: string;
    convertedFunctionDDL: string;
    convertedStoredProcedureDDL: string;
    convertedDmsJson: string;

    zipAsBase64: string;
}


export class SqlServerConverter extends React.Component<RouteComponentProps<{}>, SqlServerConverterState> {
    constructor(props: any) {
        super(props);
        this.state = {
            copied: false, loading: false, converting: false, convertToDatabaseType: "Postgres", convertToSnakeCase: true,
            unconvertedTableDDL: "",
            unconvertedTableTypeDDL: "",
            unconvertedViewDDL: "",
            unconvertedFunctionDDL: "",
            unconvertedStoredProcedureDDL: "",
            converted: {
                convertedTableDDL: "",
                convertedTableTypeDDL: "",
                convertedFunctionDDL: "",
                convertedViewDDL: "",
                convertedStoredProcedureDDL: "", convertedDmsJson: "", zipAsBase64: ""
            }
        }; 
       
        
    }

    public render() {
        
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderForm();

        return <div>
          
            { contents }
        </div>;
    }
    
    public loadSampleStoredProcs(samplesqlStoredProcPath: string) {

        fetch(samplesqlStoredProcPath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unconvertedStoredProcedureDDL + text;
                this.setState({ unconvertedStoredProcedureDDL: text });
            });
    }
    public loadSampleTables(samplesqlPath : string)
    {
        fetch(samplesqlPath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unconvertedTableDDL + text;
                this.setState({ unconvertedTableDDL: text });
            });

    }
    public loadSampleViews(samplesqlPath: string) {
        fetch(samplesqlPath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unconvertedViewDDL + text;
                this.setState({ unconvertedViewDDL: text });
            });

    }

    public loadSampleFunctions(samplesqlPath: string) {
        fetch(samplesqlPath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unconvertedFunctionDDL + text;
                this.setState({ unconvertedFunctionDDL: text });
            });

    }


    public loadSampleTableTypes(samplesqlTableTypePath: string) {
        fetch(samplesqlTableTypePath)
            .then(response => response.text())
            .then(text => {
                text = this.state.unconvertedTableTypeDDL + text;
                this.setState({ unconvertedTableTypeDDL: text });
            });

    }

    public loadSample() {

        this.loadSampleTables(require("../Sample/Sql/Sample1/Tables.sql"));


        this.loadSampleTableTypes(require("../Sample/Sql/Sample1/TableTypes.sql"));

        this.loadSampleStoredProcs(require("../Sample/Sql/Sample1/StoredProcs.sql"));
    }

    handleChangeTable(e: any ) {
        var val: string = e.target.value;
        
        this.setState({ unconvertedTableDDL:val });
        //this.setState({ [e.target.id]: e.target.value });

    }

    handleChangeTableType(e: any) {
        var val: string = e.target.value;

        this.setState({ unconvertedTableTypeDDL: val });
        //this.setState({ [e.target.id]: e.target.value });

    }
    handleChangeFunction(e: any) {
        var val: string = e.target.value;

        this.setState({ unconvertedFunctionDDL: val });
        //this.setState({ [e.target.id]: e.target.value });

    }
    handleChangeView(e: any) {
        var val: string = e.target.value;

        this.setState({ unconvertedViewDDL: val });
        //this.setState({ [e.target.id]: e.target.value });

    }
    handleChangeStoredProc(e: any) {
        var val: string = e.target.value;

        this.setState({ unconvertedStoredProcedureDDL: val });
        //this.setState({ [e.target.id]: e.target.value });

    }

    onCopy() {
        this.setState({ copied: true });
    }
    public reset() {
        
        this.setState({ unconvertedTableDDL: "" });
        this.setState({ unconvertedTableTypeDDL: "" });
        this.setState({ unconvertedStoredProcedureDDL: "" });

        let j = this.state.converted;
        j.convertedTableDDL = "";
        this.setState({ converted: j });
    }

    public downloadFiles() {
        this.download("Flyway.zip",this.state.converted.zipAsBase64 );
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
        this.setState({ converting: true });

        this.postData('api/SqlServerConverter/Convert', {
            ConvertToDatabaseType: this.state.convertToDatabaseType,
            ConvertToSnakeCase: this.state.convertToSnakeCase,
            UnconvertedTableDDL: this.state.unconvertedTableDDL,
            UnconvertedTableTypeDDL: this.state.unconvertedTableTypeDDL,
            UnconvertedStoredProcedureDDL: this.state.unconvertedStoredProcedureDDL
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
                response.json() as Promise<Converted>
            )
            .then((data) => {
                let c = this.state.converted;
                c.convertedTableDDL = data.convertedTableDDL;
                c.convertedTableTypeDDL = data.convertedTableTypeDDL;
                c.convertedViewDDL = data.convertedViewDDL;
                c.convertedFunctionDDL = data.convertedFunctionDDL;
                c.convertedStoredProcedureDDL = data.convertedStoredProcedureDDL;
                c.convertedDmsJson = data.convertedDmsJson;
                c.zipAsBase64 = data.zipAsBase64;
                this.setState({ converted: c });
                this.setState({ converting: false });
            });
};
    handleDbmsChange(event: any) {
        let value = event.target.value;
        this.setState({
            convertToDatabaseType: value
        });
    }

    private renderForm() {
        alert("hi");
        let kickstartModel = {} as SqlServerConverterModel;
        kickstartModel.CreateDatabaseProject = false;

        var convertbutton;
        if (this.state.converting) {
            convertbutton = <button type="button" value="Converting" onClick={e => this.handleClick(e)}>Converting</button>
        }
        else {
            convertbutton = <button type="button" value="Convert" onClick={e => this.handleClick(e)}>Convert</button>

        }

        var results;

        if (this.state.converted.convertedTableDDL.length > 0) {
            results =
                <div>
                <Tabs>
                    <TabList>
                        <Tab>Tables</Tab>
                        <Tab>Table Types</Tab>
                        <Tab>Functions</Tab>
                        <Tab>Views</Tab>
                        <Tab>Stored Procs</Tab>
                        <Tab>DMS Json</Tab>
                    </TabList>

                    <TabPanel>
                        <textarea value={this.state.converted.convertedTableDDL}
                            onChange={({ target: { value } }) => { let c = this.state.converted; c.convertedTableDDL = value; this.setState({ converted: c, copied: false }) }}
                        />

                    </TabPanel>
                    <TabPanel>
                        <textarea value={this.state.converted.convertedTableTypeDDL}
                            onChange={({ target: { value } }) => { let c = this.state.converted; c.convertedTableTypeDDL = value; this.setState({ converted: c, copied: false }) }}
                        />

                    </TabPanel>
                    <TabPanel>
                        <textarea value={this.state.converted.convertedFunctionDDL}
                            onChange={({ target: { value } }) => { let c = this.state.converted; c.convertedFunctionDDL = value; this.setState({ converted: c, copied: false }) }}
                        />

                    </TabPanel>
                    <TabPanel>
                        <textarea value={this.state.converted.convertedViewDDL}
                            onChange={({ target: { value } }) => { let c = this.state.converted; c.convertedViewDDL = value; this.setState({ converted: c, copied: false }) }}
                        />

                    </TabPanel>

                    <TabPanel>
                        <textarea value={this.state.converted.convertedStoredProcedureDDL}
                            onChange={({ target: { value } }) => { let c = this.state.converted; c.convertedStoredProcedureDDL = value; this.setState({ converted: c, copied: false }) }}
                        />

                    </TabPanel>
                    <TabPanel>
                        <textarea value={this.state.converted.convertedDmsJson}
                            onChange={({ target: { value } }) => { let c = this.state.converted; c.convertedDmsJson = value; this.setState({ converted: c, copied: false }) }}
                        />

                    </TabPanel>

                </Tabs>
                <button onClick={e=> this.downloadFiles()} >Download Flyway Files</button>
                </div>
        }

        return (
            <form className="form-labels-on-top">
                <div className="form-title-row">
                    <h1>Sql Server Converter</h1>
                </div>


                <div className="form-labels-on-top">
                    
                    
                    <div className="form-row">
                        <label><span>Target Data Store</span></label>
                        <div>
                            <label>
                                <select onChange={(e) => this.handleDbmsChange(e)}>
                                    <option value="Postgres">Postgresql 9.6</option>
                                    <option value="MySql">MySql</option> 
                                    <option value="SqlServer">Sql Server 2014</option>
                                </select>
                            </label>
                        </div>
                        <label><span>Snake Case</span></label>
                        <div>
                            <input type="checkbox" defaultChecked
                                onChange={({ target: { checked } }) => this.setState({ convertToSnakeCase: checked })}

                            />
                        </div>
                    </div>

                    <div className="form-row">
                        <label><span>TSQL DDL</span></label>
                        <div>
                            <Tabs>
                                <TabList>
                                    <Tab>Tables</Tab>
                                    <Tab>Table Types</Tab>
                                    <Tab>Functions</Tab>
                                    <Tab>Views</Tab>

                                    <Tab>Stored Procs</Tab>
                                </TabList>

                                <TabPanel>
                                    <label>
                                        <textarea name="TextAreaUnconvertedTableDDL" value={this.state.unconvertedTableDDL} onChange={this.handleChangeTable.bind(this)}   ></textarea>

                                    </label>
                                    <button type="button" onClick={e => this.loadSample()}> Load Sample 1</button>
                                  


                                </TabPanel>
                                <TabPanel>
                                    <label>
                                        <textarea name="TextUnconvertedTableTypeDDL" value={this.state.unconvertedTableTypeDDL} onChange={this.handleChangeTableType.bind(this)}  ></textarea>

                                    </label>
                                    
                                </TabPanel>
                                <TabPanel>
                                    <label>
                                        <textarea name="TextUnconvertedFunctionDDL" value={this.state.unconvertedFunctionDDL} onChange={this.handleChangeFunction.bind(this)}  ></textarea>

                                    </label>

                                </TabPanel>
                                <TabPanel>
                                    <label>
                                        <textarea name="TextUnconvertedViewDDL" value={this.state.unconvertedViewDDL} onChange={this.handleChangeView.bind(this)}  ></textarea>

                                    </label>

                                </TabPanel>
                                <TabPanel>
                                    <label>
                                        <textarea name="UnconvertedStoredProcedureDDL" value={this.state.unconvertedStoredProcedureDDL} onChange={this.handleChangeStoredProc.bind(this)}  ></textarea>

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
                <p>Foreign Keys, Indexes, and Default Values are not converted (yet)</p>
                    </form>
    
        );
    }
}



