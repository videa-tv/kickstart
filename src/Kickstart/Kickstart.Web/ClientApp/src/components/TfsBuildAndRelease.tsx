import '../css/form-labels-on-top.css';
import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import 'isomorphic-fetch';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import 'react-tabs/style/react-tabs.css';

interface KickstartWizardExampleState {
    loading: boolean;
    errorMessage: string,
    generationModel: GenerationModel;
}

interface GenerationModel {
    buildFileText: string,
    releaseDefinitionFileText : string
}



export class TfsBuildAndRelease extends React.Component<RouteComponentProps<{}>, KickstartWizardExampleState> {
    constructor(props: any) {
        super(props);
        this.state = {
            loading: false, errorMessage: "", generationModel: {
                buildFileText: "",
                releaseDefinitionFileText: ""
            }
        };


    }


    public loadSample() {

        this.loadSampleBuildFile(require("../Sample/BuildAndRelease/PartyModel/DEV_QA/CreateBuildDefinition.json"));

        this.loadSampleReleaseDefinitionFile(require("../Sample/BuildAndRelease/PartyModel/DEV_QA/CreateReleaseDefinition.json"));

    }


    public loadSampleBuildFile(sampleProtoPath: string) {
        fetch(sampleProtoPath)
            .then(response => response.text())
            .then(text => {
                //text = this.state.unconvertedTableDDL + text;
                let g = this.state.generationModel;
                g.buildFileText = text;
                this.setState({ generationModel: g });
            });

    }

    public loadSampleReleaseDefinitionFile(sampleProtoPath: string) {
        fetch(sampleProtoPath)
            .then(response => response.text())
            .then(text => {
                //text = this.state.unconvertedTableDDL + text;
                let g = this.state.generationModel;
                g.releaseDefinitionFileText = text;
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
        g.buildFileText = val;
        this.setState({ generationModel: g });
        //this.setState({ [e.target.id]: e.target.value });

    }

    handleChange2(e: any) {
        var val: string = e.target.value;
        let g = this.state.generationModel;
        g.releaseDefinitionFileText = val;
        this.setState({ generationModel: g });
        //this.setState({ [e.target.id]: e.target.value });

    }

    handleClickGenerateBuild(e: React.MouseEvent<HTMLButtonElement>) {
        e.preventDefault();
        //alert(kickstartModel.CreateDatabaseProject);

        //alert(this.state.generationModel.buildFileText);

        var json = JSON.parse(this.state.generationModel.buildFileText);
        //alert(JSON.stringify( json));
        this.postData('api/TfsBuild/GenerateBuild', json);

    }

    handleClickGenerateRelease(e: React.MouseEvent<HTMLButtonElement>) {
        e.preventDefault();
        //alert(kickstartModel.CreateDatabaseProject);

        //alert('The link was clicked.');
        var json = JSON.parse(this.state.generationModel.releaseDefinitionFileText);

        this.postData('api/TfsRelease/GenerateRelease', json);

    }


    public postData(url: string, data: object) {
        //alert(JSON.stringify(data));
        fetch(url,
            {
                headers: {
                    'content-type': 'application/json'
                },
                method: "post",
                body: JSON.stringify(data)
            }
        );
    }

    private renderForm() {
        var results;
        var errorMessage;

        errorMessage = <label>{this.state.errorMessage}</label>
    
        //if (1 == 1)
        {
            results =
                <form className="form-labels-on-top">
                    <div className="form-title-row">
                        <h1>Tfs Build and Release Wizard</h1>A
                    </div>
                    <Tabs>
                        <TabList>
                            <Tab>Build Def Template Json</Tab>
                            <Tab>Release Def Template Json</Tab>
                        </TabList>

                        <TabPanel>


                            <div className="form-row">
                                <label>
                                    <span> Json </span>
                                    <textarea name="protoText" value={this.state.generationModel.buildFileText} onChange={this.handleChange1.bind(this)} />
                                </label>
                            </div>
                            <div className="form-row">

                                <button type="button" onClick={e => this.loadSample()}> Load Sample-> Party Model Svc</button>
                               


                            </div>
                        </TabPanel>
                        <TabPanel>
                            <div className="form-row">
                                <label>
                                    <span> Json </span>
                                    <textarea name="protoText" value={this.state.generationModel.releaseDefinitionFileText} onChange={this.handleChange2.bind(this)} />
                                </label>
                            </div>
                        </TabPanel>
                    </Tabs>
                    <div className="form-title-row">
                        <button type="button" value="Submit" onClick={e => this.handleClickGenerateBuild(e)}>Generate Build Def</button>
                        <button type="button" value="Submit" onClick={e => this.handleClickGenerateRelease(e)}>Generate Release Def</button>

                        {errorMessage}
                    </div>

                </form>
        }

        return (<div>{results}</div>);
    }
}




