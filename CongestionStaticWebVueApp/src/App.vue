<template>
    <div class="container">
        <table class="table table-bordered">
            <caption>Congestion Status</caption>
            <thead class="thead-light">
                <tr class="place-header">
                    <th scope="col">Place</th>
                    <th scope="col">Count</th>
                    <th scope="col">Trend</th>
                </tr>
            </thead>
            <tr v-for="result in results" :key="result.id">
                <td class="place-name">
                    <span hidden>{{ result.id }}</span>
                    <span>{{ result.placeName }}</span>
                </td>
                <td class="people-count">
                    <transition name="fade" mode="out-in">
                        <span :key="result.faceCount">
                            {{ result.faceCount }}
                        </span>
                    </transition>
                </td>
                <td class="trend-indicator">
                    <transition name="mark" mode="out-in">
                        <span v-bind:style="{ color: result.indicatorColor }" :key="result.updown">
                            {{ result.updown }}
                        </span>
                    </transition>
                </td>
            </tr>
        </table>
    </div>
</template>

<script>
import Vue from 'vue';
import axios from 'axios';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

const apiBaseUrl = 'https://functionappnoodlefinder2.azurewebsites.net';
//const apiBaseUrl = 'http://localhost:7071';
const axiosConfig = {};
const indicator = {
    up: {
        mark: '▲',
        color: 'rgb(255, 148, 148)'
    },
    down: {
        mark: '▼',
        color: 'rgb(154, 240, 117)'
    },
    stay: {
        mark: '－',
        color: 'gray'
    }
};

export default {
    name: "App",
    data() {
        return {
            results: []
        };
    },
    created() {
        var self = this;
        this.getCurrentData().then(function (datas) {
            datas.forEach(self.faceCountUpdated);
        }).then(self.getConnectionInfo).then(function (info) {
            let accessToken = info.accessToken;
            const options = {
                accessTokenFactory: function () {
                    if (accessToken) {
                        const _accessToken = accessToken;
                        accessToken = null;
                        return _accessToken;
                    } else {
                        return self.getConnectionInfo().then(function (info) {
                            return info.accessToken;
                        });
                    }
                }
            };

            // Setup SignalR connection.
            const connection = new HubConnectionBuilder()
                .withUrl(info.url, options)
                .configureLogging(LogLevel.Information)
                .build();

            connection.on('faceCountUpdated', self.faceCountUpdated);

            connection.onclose(function () {
                console.log('disconnected');
                setTimeout(function () { self.startConnection(connection); }, 2000);
            });

            self.startConnection(connection);

        }).catch(console.error);
    },
    methods: {
        getCurrentData() {
            return axios.post(`${apiBaseUrl}/api/GetCurrentFaceData`, null, axiosConfig)
                .then(function (resp) { return resp.data; })
                .catch(function () { return {}; });
        },
        getConnectionInfo() {
            return axios.post(`${apiBaseUrl}/api/GetSignalRInfo`, null, axiosConfig)
                .then(function (resp) { return resp.data; })
                .catch(function () { return {}; });
        },
        startConnection(connection) {
            console.log('connecting...');
            connection.start()
                .then(function () { console.log('connected!'); })
                .catch(function (err) {
                    console.error(err);
                    setTimeout(function () { this.startConnection(connection); }, 2000);
                });
        },
        faceCountUpdated(countResult) {

            const result = this.results.find(f => f.id === countResult.id);
            if (result) {

                if (result.faceCount < countResult.faceCount) {
                    Vue.set(result, 'updown', indicator.up.mark);
                    Vue.set(result, 'indicatorColor', indicator.up.color);
                }
                else if (result.faceCount > countResult.faceCount) {
                    Vue.set(result, 'updown', indicator.down.mark);
                    Vue.set(result, 'indicatorColor', indicator.down.color);
                }

                Vue.set(result, 'faceCount', countResult.faceCount);

            } else {
                countResult.updown = indicator.stay.mark;
                countResult.indicatorColor = indicator.stay.color;
                this.results.push(countResult);
            }
        }
    }
}
</script>

<style>
</style>

