export function Test() {
  let ws: WebSocket;
  function sendMsg() {
    const json = { type: "msg", message: "msg" };
    ws.send(JSON.stringify(json));
  }
  function connect() {
    ws = new WebSocket("ws://localhost:3000/chat/12232");
  }
  return (
    <>
      <button onClick={sendMsg}>send message</button>
      <button onClick={connect}>connect</button>
    </>
  );
}
