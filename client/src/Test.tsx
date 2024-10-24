export function Test() {
    fetch("http://localhost:3000/123213")
  let ws: WebSocket = new WebSocket("ws://localhost:3000/ws", ["token"]);
  function killWs() {
    ws.close();
  }
  function sendMsg() {
    const json = { type: "msg", message: "msg" };
    ws.send(JSON.stringify(json));
  }
  function connect() {
    ws = new WebSocket("ws://localhost:3000/chat/1232", ["token"]);
    ws.addEventListener("message", (event) => {
      console.log("Data from server", JSON.parse(event.data));
    });
  }
  return (
    <>
      <button onClick={sendMsg}>send message</button>
      <button onClick={killWs}>killws</button>
      <button onClick={connect}>connect</button>
    </>
  );
}
