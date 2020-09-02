module.exports = async function (context, req) {
    const responseMessage = "Welcome to Congestion Monitor Project!"
    context.res = {
        body: responseMessage
    };
}