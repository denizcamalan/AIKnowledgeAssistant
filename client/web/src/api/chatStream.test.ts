import { describe, expect, it, vi } from "vitest";
import { consumeSse } from "./chatStream";

describe("consumeSse", () => {
  it("parses started, delta, and done events", async () => {
    const sse =
      'event: started\ndata: {"streamId":"11111111-1111-1111-1111-111111111111"}\n\n' +
      'event: delta\ndata: {"text":"Hi"}\n\n' +
      'event: done\ndata: {"streamId":"11111111-1111-1111-1111-111111111111","model":"qwen3:4b"}\n\n';

    const onStarted = vi.fn();
    const onDelta = vi.fn();
    const onDone = vi.fn();

    await consumeSse(toStream(sse), { onStarted, onDelta, onDone });

    expect(onStarted).toHaveBeenCalledWith("11111111-1111-1111-1111-111111111111");
    expect(onDelta).toHaveBeenCalledWith("Hi");
    expect(onDone).toHaveBeenCalledWith(
      expect.objectContaining({ model: "qwen3:4b" }),
    );
  });
});

function toStream(text: string): ReadableStream<Uint8Array> {
  return new ReadableStream({
    start(controller) {
      controller.enqueue(new TextEncoder().encode(text));
      controller.close();
    },
  });
}
