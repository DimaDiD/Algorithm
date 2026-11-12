import { useState } from "react";
import { fireEvent, render, screen } from "@testing-library/react";
import ErrorBoundary, { isChunkLoadError } from "./ErrorBoundary";

const Bomb = ({ shouldThrow }: { shouldThrow: boolean }) => {
  if (shouldThrow) throw new Error("boom");
  return <p>Вміст</p>;
};

const Harness = () => {
  const [shouldThrow, setShouldThrow] = useState(true);
  return (
    <ErrorBoundary
      fallback={({ reset }) => (
        <button
          type="button"
          onClick={() => {
            setShouldThrow(false);
            reset();
          }}
        >
          Спробувати ще раз
        </button>
      )}
    >
      <Bomb shouldThrow={shouldThrow} />
    </ErrorBoundary>
  );
};

describe("ErrorBoundary", () => {
  beforeEach(() => {
    jest.spyOn(console, "error").mockImplementation(() => undefined);
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it("shows the fallback and recovers after reset", () => {
    render(<Harness />);
    expect(screen.queryByText("Вміст")).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Спробувати ще раз" }));
    expect(screen.getByText("Вміст")).toBeInTheDocument();
  });

  it("resets when resetKey changes", () => {
    const fallback = () => <p>Помилка</p>;
    const { rerender } = render(
      <ErrorBoundary resetKey={1} fallback={fallback}>
        <Bomb shouldThrow />
      </ErrorBoundary>
    );
    expect(screen.getByText("Помилка")).toBeInTheDocument();

    rerender(
      <ErrorBoundary resetKey={2} fallback={fallback}>
        <Bomb shouldThrow={false} />
      </ErrorBoundary>
    );
    expect(screen.getByText("Вміст")).toBeInTheDocument();
  });
});

describe("isChunkLoadError", () => {
  it("detects webpack chunk failures", () => {
    const error = new Error("Loading chunk 594 failed.");
    error.name = "ChunkLoadError";
    expect(isChunkLoadError(error)).toBe(true);
  });

  it("ignores ordinary errors", () => {
    expect(isChunkLoadError(new Error("boom"))).toBe(false);
  });
});
