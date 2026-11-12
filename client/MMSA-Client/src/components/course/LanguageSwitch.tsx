import { Segmented } from "antd";
import { useTable } from "../../Store";
import { pickCodeOption, useCourseLanguages } from "../../course/useCourse";

type Props = {
  variant?: "light" | "dark";
  codes?: string[];
};

const LanguageSwitch = ({ variant = "light", codes }: Props) => {
  const [state, actions] = useTable();
  const courseLanguages = useCourseLanguages();

  const source = codes ? state.languages.filter((language) => codes.includes(language.Code)) : courseLanguages;
  const options = source.map((language) => ({ label: language.Name, value: language.Code }));
  const value = pickCodeOption(state.codeOption, options.map((option) => option.value));

  return (
    <Segmented
      aria-label="Мова прикладів коду"
      className={`lang-switch${variant === "dark" ? " lang-switch--dark" : ""}`}
      options={options}
      value={value}
      onChange={(next) => actions.setCodeOption(String(next))}
    />
  );
};

export default LanguageSwitch;
