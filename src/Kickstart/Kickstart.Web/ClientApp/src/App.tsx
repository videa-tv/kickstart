import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import { KickstartWizard } from './components/KickstartWizard';
import { SqlServerConverter } from './components/SqlServerConverter';
import { FlywaySplitter } from './components/FlywaySplitter';
import { TfsBuildAndRelease } from './components/TfsBuildAndRelease';

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    render() {
        return (
            <Layout>
                <Route exact path='/' component={Home} />
                <Route path='/counter' component={Counter} />
                <Route path='/fetch-data' component={FetchData} />
                <Route path='/kickstartwizard' component={KickstartWizard} />
                <Route path='/flywaysplitter' component={FlywaySplitter} />
                <Route path='/tfsbuildandrelease' component={TfsBuildAndRelease} />

            </Layout>
        );
    }
}
