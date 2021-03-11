<template>
    <div class="container">
        <table class="table table-bordered">
            <caption>Congestion Status</caption>
            <thead class="thead-light">
                <tr class="place-header">
                    <th scope="col">Place</th>
                    <th scope="col">Count</th>
                    <th scope="col">Trend</th>
                    <th scope="col">Mask</th>
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
                <td class="mask-ratio">
                    <transition name="fade" mode="out-in">
                        <span :key="result.maskRatio">
                            {{ result.maskRatio }} % {{ result.maskStatus }}
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

    const apiBaseUrl = process.env.VUE_APP_API_BASE_URL;
    const axiosConfig = {};
    const indicator = {
        up: {
            mark: '▲',
            color: 'rgb(232, 18, 36)'
        },
        down: {
            mark: '▼',
            color: 'rgb(0, 120, 215)'
        },
        stay: {
            mark: '－',
            color: 'gray'
        }
    };
    const status = {
        awesome: '😍',
        high: '😃',
        middle: '😐',
        low: '🙁',
        bad: '😰'
    };

    export default {
        name: "App",
        data() {
            return {
                results: []
            };
        },
        created() {
            console.log('NODE_ENV=' + process.env.NODE_ENV);
            console.log('API_BASE_URL=' + apiBaseUrl);

            var self = this;
            self.getCurrentData().then(function (datas) {
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
                return axios.post(`${apiBaseUrl}/api/SignalRInfo`, null, axiosConfig)
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

                    var ratio = this.getMaskRatio(countResult);
                    var status = this.getMaskStatus(countResult, ratio);
                    Vue.set(result, 'maskRatio', ratio);
                    Vue.set(result, 'maskStatus', status);
                    Vue.set(result, 'faceCount', countResult.faceCount);

                } else {
                    countResult.maskRatio = this.getMaskRatio(countResult);
                    countResult.maskStatus = this.getMaskStatus(countResult, countResult.maskRatio);
                    countResult.updown = indicator.stay.mark;
                    countResult.indicatorColor = indicator.stay.color;
                    this.results.push(countResult);
                }
            },
            getMaskRatio(result) {
                if (result.faceCount > 0)
                    return Math.round(result.maskCount / result.faceCount * 100);
                else
                    return 0;
            },
            getMaskStatus(result, ratio) {
                if (result.faceCount == 0)
                    return status.middle;
                else if (ratio >= 100)
                    return status.awesome;
                else if (ratio > 80)
                    return status.high;
                else if (ratio > 50)
                    return status.middle;
                else if (ratio > 20)
                    return status.low;
                else
                    return status.bad;
            }
        }
    }
</script>

<style>
</style>
