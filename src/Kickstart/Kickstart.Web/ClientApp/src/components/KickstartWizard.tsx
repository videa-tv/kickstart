import '../css/form-labels-on-top.css';
import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import 'isomorphic-fetch';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import 'react-tabs/style/react-tabs.css';

interface KickstartWizardExampleState {
    loading: boolean;
    generationModel: GenerationModel;
    generatedSolution: GeneratedSolution;
}

interface GenerationModel {
    databaseType: number,
    generateDatabaseProject: boolean,
    generateDataLayerProject: boolean,
    generateGrpcServiceProject: boolean,
    generateWebAppProject: boolean,
    generateGrpcUnitTestProject: boolean,
    generateDockerComposeProject: boolean,
    generateGrpcClientProject: boolean,
    companyName: string,
    solutionName: string,
    projectName: string,
    protoFileText: string,
    convertToSnakeCase: boolean
}

interface GeneratedSolution {
    succeeded: boolean,
    errorMessage: string,
    zipAsBase64: string;
}



export class KickstartWizard extends React.Component<RouteComponentProps<{}>, KickstartWizardExampleState> {
    constructor(props :any) {
        super(props);
        this.state = {
            loading: false, generationModel: {
                generateDatabaseProject: true, generateDataLayerProject: true, generateGrpcServiceProject: true,
                generateWebAppProject: true, generateGrpcUnitTestProject: true, generateDockerComposeProject: true,
                generateGrpcClientProject: true,
                databaseType: 1, companyName: "", projectName: "", solutionName: "", protoFileText: "", convertToSnakeCase: true
            }, generatedSolution: { succeeded: false, errorMessage: "", zipAsBase64: "" }
        };


    }

    
    public loadSample1() {
        this.state.generationModel.companyName = "Company";
        this.state.generationModel.solutionName = "PartyModel";
        this.state.generationModel.projectName = "PartyModel";
        this.state.generationModel.databaseType = 1;
        this.state.generationModel.generateWebAppProject = false;
        this.state.generationModel.generateGrpcClientProject = false;
        this.loadSampleProtoFile(require("../Sample/Proto/PartyModel.proto"));
    }

 

    public loadSampleProtoFile(sampleProtoPath: string) {
        fetch(sampleProtoPath)
            .then(response => response.text())
            .then(text => {
                //text = this.state.unconvertedTableDDL + text;
                let g = this.state.generationModel;
                g.protoFileText = text;
                this.setState({ generationModel: g });
            });

    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderForm();

        return <div>
            {contents}
        </div>;
    }
    handleChange1(e: any) {
        var val: string = e.target.value;
        let g = this.state.generationModel;
        g.protoFileText = val;
        this.setState({ generationModel: g });
        //this.setState({ [e.target.id]: e.target.value });

    }
    handleDbmsChange(event: any) {
        let value = event.target.value;
        let g = this.state.generationModel;
        g.databaseType = value;
        this.setState({ generationModel: g });
    }
    public downloadFiles() {
        this.download("GeneratedSolution.zip", this.state.generatedSolution.zipAsBase64);
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


    handleClick(e: React.MouseEvent<HTMLButtonElement>) {
        e.preventDefault();
        //alert(kickstartModel.CreateDatabaseProject);

        //alert('The link was clicked.');
        this.postData('api/KickstartWizard/BuildSolution', {
            ProtoFileText: this.state.generationModel.protoFileText,
            CompanyName: this.state.generationModel.companyName,
            SolutionName: this.state.generationModel.solutionName,
            ProjectName: this.state.generationModel.projectName,
            DatabaseType: this.state.generationModel.databaseType,
            GenerateDatabaseProject: this.state.generationModel.generateDatabaseProject,
            GenerateDataLayerProject: this.state.generationModel.generateDataLayerProject,
            GenerateGrpcServiceProject: this.state.generationModel.generateGrpcServiceProject,
            GenerateDockerComposeProject: this.state.generationModel.generateDockerComposeProject,
            GenerateGrpcClientProject: this.state.generationModel.generateGrpcClientProject,
            GenerateWebAppProject: this.state.generationModel.generateWebAppProject,
            ConvertToSnakeCase: this.state.generationModel.convertToSnakeCase
        });

    }

    handleGenerateDatabaseProjectChange(event:any) {
        let c = this.state.generationModel;
        c.generateDatabaseProject = event.target.checked;
        this.setState({ generationModel: c });
    }

    handleGenerateDataLayerProjectChange(event: any) {
        let c = this.state.generationModel;
        c.generateDataLayerProject = event.target.checked;
        this.setState({ generationModel: c });
    }

    handleGenerateGrpcServiceProjectChange(event: any) {
        let c = this.state.generationModel;
        c.generateGrpcServiceProject = event.target.checked;
        this.setState({ generationModel: c });
    }

    

    handleGenerateWebAppProjectChange(event: any) {
        let c = this.state.generationModel;
        c.generateWebAppProject = event.target.checked;
        this.setState({ generationModel: c });
    }
    
    handleGenerateDockerComposeProjectChange(event: any) {
        let c = this.state.generationModel;
        c.generateDockerComposeProject = event.target.checked;
        this.setState({ generationModel: c });
    }
    handleGenerateGrpcClientProjectChange(event: any) {
        let c = this.state.generationModel;
        c.generateGrpcClientProject = event.target.checked;
        this.setState({ generationModel: c });
    }
    handleGenerateGrpcUnitTestProjectChange(event: any) {
        let c = this.state.generationModel;
        c.generateGrpcUnitTestProject = event.target.checked;
        this.setState({ generationModel: c });
    }

    
    public postData(url: string, data: object) {
        fetch(url,
            {
                headers: {
                    'content-type': 'application/json'
                },
                method: "post",
                body: JSON.stringify(data)
            }
        )
            .then(response => response.json() as Promise<GeneratedSolution>)
            .then(data => {
                let s = this.state.generatedSolution;
                s.succeeded = data.succeeded;
                s.errorMessage = data.errorMessage;
                s.zipAsBase64 = data.zipAsBase64;
                this.setState({ generatedSolution: s });
            });
    }

    private renderForm() {


        var results;

        var errorMessage;

        if (this.state.generatedSolution.succeeded == false) {
            errorMessage = <label>{this.state.generatedSolution.errorMessage}</label>
        }
        var downloadButton;
        if (this.state.generatedSolution.zipAsBase64.length > 0) {

            downloadButton = <button onClick={e => this.downloadFiles()} >Download GeneratedSolution</button>
        }

        //if (1 == 1)
        {
            results =
                <form className="form-labels-on-top">
                    <div className="form-title-row">
                        <h1>Kickstart Wizard</h1>
                    </div>
                    <Tabs>
                        <TabList>
                            <Tab>Input</Tab>
                            <Tab>Options</Tab>
                            <Tab>Sample</Tab>

                        </TabList>

                        <TabPanel>
                            <div className="form-row">
                                <label>
                                    <span>Company Name</span>
                                    <input type="text" name="companyName" value={this.state.generationModel.companyName}
                                        placeholder="Enter company name"
                                        onChange={({ target: { value } }) => {
                                            let c = this.state.generationModel; c.companyName = value;
                                            this.setState({ generationModel: c })
                                        }}></input>
                                </label>
                            </div>

                            <div className="form-row">

                                <label>
                                    <span>Solution Name</span>
                                    <input type="text" name="solutionName" value={this.state.generationModel.solutionName}
                                        placeholder="Enter solution name"
                                        onChange={({ target: { value } }) => {
                                            let c = this.state.generationModel; c.solutionName = value;
                                            this.setState({ generationModel: c })
                                        }}></input>



                                </label>
                            </div>
                            <div className="form-row">

                                <label>
                                    <span>Project Names</span>
                                    <input type="text" name="projectName" value={this.state.generationModel.projectName}
                                        placeholder="Enter project name"
                                        onChange={({ target: { value } }) => {
                                            let c = this.state.generationModel; c.projectName = value;
                                            this.setState({ generationModel: c })
                                        }}></input>
                                </label>
                            </div>

                            <div className="form-row">
                                <label>
                                    <span> Proto File</span>
                                    <textarea name="protoText" value={this.state.generationModel.protoFileText} onChange={this.handleChange1.bind(this)} />
                                </label>
                            </div>

                        </TabPanel>
                        <TabPanel>
                            <div className="form-row">
                                <label><span>Target Data Store</span></label>
                                <div>
                                    <label>
                                        <select value={this.state.generationModel.databaseType} onChange={(e) => this.handleDbmsChange(e)}>
                                            <option value="1">Postgresql 9.6</option>
                                            <option value="2">Sql Server 2014</option>
                                            <option value="3">MySql</option>
                                            <option value="4">Kinesis</option>
                                            <option value="5">Kafka</option>
                                            <option value="6">GenericResource</option>
                                            <option value="7">Tfs</option>
                                            <option value="8">Okta</option>
                                        </select>
                                    </label>
                                </div>

                            </div>
                            <div className="form-row">
                                
                                <div>
                                <input id="generateDatabaseProject"
                                    type="checkbox"
                                    checked={this.state.generationModel.generateDatabaseProject}
                                    onClick={e => this.handleGenerateDatabaseProjectChange(e)}>
                                </input>
                                <label htmlFor="generateDatabaseProject" >Generate Database Project</label>
                                </div>
                            </div>
                            <div className="form-row">
                                <div>
                                    <input id="generateDataLayerProject"
                                           type="checkbox"
                                           checked={this.state.generationModel.generateDataLayerProject}
                                    onClick={e => this.handleGenerateDataLayerProjectChange(e)}>
                                    </input>
                                    <label htmlFor="generateDataLayerProject" >Generate Data Layer Project</label>
                                </div>
                                
                            </div>
                            <div className="form-row">
                                <div>
                                    <input id="generateGrpcServiceProject"
                                           type="checkbox"
                                            checked={this.state.generationModel.generateGrpcServiceProject}
                                    onClick={e => this.handleGenerateGrpcServiceProjectChange(e)}>
                                    </input>
                                <label htmlFor="generateGrpcServiceProject" >Generate Grpc ServiceProject</label>
                                </div>

                            </div>
                            <div className="form-row">
                                <div>
                                    <input id="generateGrpcUnitTestProject"
                                           type="checkbox"
                                           checked={this.state.generationModel.generateGrpcUnitTestProject}
                                           onClick={e => this.handleGenerateGrpcUnitTestProjectChange(e)}>
                                    </input>
                                    <label htmlFor="generateWebAppProject" >Generate Grpc Unit Test Project</label>
                                </div>

                            </div>
                            <div className="form-row">
                                <div>
                                <input id="generateWebAppProject"
                                           type="checkbox"
                                            checked={this.state.generationModel.generateWebAppProject}
                                            onClick={e => this.handleGenerateWebAppProjectChange(e)}>
                                    </input>
                                <label htmlFor="generateWebAppProject" >Generate Web App Project</label>
                                </div>

                            </div>
                            <div className="form-row">
                                <div>
                                    <input id="generateDockerComposeProject"
                                           type="checkbox"
                                        checked={this.state.generationModel.generateDockerComposeProject}
                                        onClick={e => this.handleGenerateDockerComposeProjectChange(e)}>
                                    </input>
                                <label htmlFor="generateDockerComposeProject" >Generate Docker Compose Project</label>
                                </div>

                            </div>
                            <div className="form-row">
                                <div>
                                <input id="generateGrpcClientProject"
                                           type="checkbox"
                                    checked={this.state.generationModel.generateGrpcClientProject}
                                    onClick={e => this.handleGenerateGrpcClientProjectChange(e)}>
                                    </input>
                                <label htmlFor="generateGrpcClientProject" >Generate Grpc Client Project</label>
                                </div>

                            </div>

                            

                        </TabPanel>
                        <TabPanel>
                            <div className="form-row">

                                <button type="button" onClick={e => this.loadSample1()}> Load Sample-> Party</button>
                        </div>
                            
                        </TabPanel>
                    </Tabs>
                    <div className="form-title-row">
                        <button type="button" value="Submit" onClick={e => this.handleClick(e)}>Generate</button>
                        {errorMessage}
                        {downloadButton}
                    </div>

                </form>
        }

        return (<div>{results}</div>);
    }
}




