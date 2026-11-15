import { useCallback, useEffect, useState } from "react";
import { Button, Modal } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import axios from "axios";
import { Exercise, ExerciseEdit, ExerciseProgress } from "../../models/Exercise";
import { MoveDirection } from "../../models/MoveDirection";
import ExerciseService from "../../services/exerciseService";
import { useCanEditCourse } from "../../course/useCourse";
import { useTable } from "../../Store";
import notification from "../../notification/notificationMessage";
import ExerciseCard from "./ExerciseCard";
import ExerciseEditorModal from "./ExerciseEditorModal";
import ExerciseStatsModal from "./ExerciseStatsModal";
import { BlockErrorBoundary } from "../common/ErrorFallbacks";

type Props = {
  pageId: number;
  subPageId?: number;
};

type EditorState = { open: false } | { open: true; exercise?: ExerciseEdit };

const HANDLED_STATUSES = [401, 403, 429];

const ExerciseSection = ({ pageId, subPageId }: Props) => {
  const [state] = useTable();
  const canEdit = useCanEditCourse();
  const [modal, contextHolder] = Modal.useModal();
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [editor, setEditor] = useState<EditorState>({ open: false });
  const [version, setVersion] = useState(0);
  const [progress, setProgress] = useState<Record<number, ExerciseProgress>>({});
  const [statsFor, setStatsFor] = useState<Exercise | null>(null);

  const isEditing = canEdit && state.isEditMode;

  const load = useCallback(async () => {
    try {
      setExercises(await ExerciseService.getForTopic(pageId, subPageId));
      setVersion((value) => value + 1);
    } catch {
      setExercises([]);
    }
  }, [pageId, subPageId]);

  const loadProgress = useCallback(async () => {
    try {
      const items = await ExerciseService.getProgress(pageId, subPageId);
      setProgress(Object.fromEntries(items.map((item) => [item.ExerciseId, item])));
    } catch {
      setProgress({});
    }
  }, [pageId, subPageId]);

  useEffect(() => {
    load();
  }, [load]);

  useEffect(() => {
    if (!canEdit) loadProgress();
  }, [canEdit, loadProgress]);

  const openEditor = async (exerciseId?: number) => {
    if (exerciseId === undefined) {
      setEditor({ open: true });
      return;
    }
    try {
      setEditor({ open: true, exercise: await ExerciseService.getForEdit(exerciseId) });
    } catch (error) {
      if (!(axios.isAxiosError(error) && HANDLED_STATUSES.includes(error.response?.status ?? 0))) {
        notification("error", "Не вдалося завантажити завдання.");
      }
    }
  };

  const confirmDelete = (exercise: Exercise) =>
    modal.confirm({
      title: `Видалити «${exercise.Title}»?`,
      content: "Завдання і всі його питання буде видалено.",
      okText: "Видалити",
      okButtonProps: { danger: true },
      cancelText: "Скасувати",
      onOk: async () => {
        try {
          await ExerciseService.remove(exercise.Id);
          notification("success", "Завдання видалено.");
          await load();
        } catch {
          notification("error", "Не вдалося видалити завдання.");
        }
      },
    });

  const move = async (exercise: Exercise, direction: MoveDirection) => {
    try {
      await ExerciseService.move(exercise.Id, direction);
      await load();
    } catch (error) {
      if (!(axios.isAxiosError(error) && HANDLED_STATUSES.includes(error.response?.status ?? 0))) {
        notification("error", "Не вдалося перемістити завдання.");
      }
    }
  };

  const handleSaved = async () => {
    setEditor({ open: false });
    notification("success", "Завдання збережено.");
    await load();
  };

  if (exercises.length === 0 && !isEditing) return null;

  return (
    <section className="exercise-section" aria-labelledby="exercise-section-title">
      <div className="exercise-section__head">
        <h2 id="exercise-section-title" className="exercise-section__title">Перевір себе</h2>
        {isEditing && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openEditor()}>
            Додати завдання
          </Button>
        )}
      </div>

      {exercises.length === 0 && (
        <div className="card exercise-empty">До цієї теми ще немає завдань.</div>
      )}

      {exercises.map((exercise, index) => (
        <BlockErrorBoundary key={`${exercise.Id}-${version}`} resetKey={exercise}>
          <ExerciseCard
            exercise={exercise}
            isEditing={isEditing}
            isCourseEditor={canEdit}
            progress={canEdit ? undefined : progress[exercise.Id]}
            onEdit={() => openEditor(exercise.Id)}
            onDelete={() => confirmDelete(exercise)}
            onMoveUp={index > 0 ? () => move(exercise, "up") : undefined}
            onMoveDown={index < exercises.length - 1 ? () => move(exercise, "down") : undefined}
            onStats={() => setStatsFor(exercise)}
            onChecked={() => {
              if (!canEdit) loadProgress();
            }}
          />
        </BlockErrorBoundary>
      ))}

      <ExerciseEditorModal
        open={editor.open}
        pageId={pageId}
        subPageId={subPageId}
        exercise={editor.open ? editor.exercise : undefined}
        onCancel={() => setEditor({ open: false })}
        onSaved={handleSaved}
      />
      <ExerciseStatsModal exercise={statsFor} onClose={() => setStatsFor(null)} />
      {contextHolder}
    </section>
  );
};

export default ExerciseSection;
