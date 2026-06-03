import { bindTriggerWithArgs, bindValue, useValue } from "cs2/api";
import { FOCUS_DISABLED, UniqueFocusKey } from "cs2/input";
import { ModuleRegistryExtend } from "cs2/modding";
import { Children, HTMLAttributes, cloneElement, isValidElement, ReactElement, ReactNode } from "react";
import mod from "../../mod.json";
import buttonImg from "../../imgs/button.svg";

const SHOW_BINDING = bindValue<boolean>(mod.id, "IsShowing");
const INVERT_MODE_BINDING = bindValue<number>(mod.id, "IsInverted");
const TOGGLE_INVERTED = bindTriggerWithArgs<[number]>(mod.id, "ToggleInverted");

const LEFT_HAND_TRAFFIC_MODE = 1;
const DEFAULT_MODE = 0;
const SECTION_TITLE = "LHT Invert Building Networks";
const TOOLTIP_TEXT = "Changes apply to all existing instances of this building.";

type ToolButtonProps = {
    focusKey?: UniqueFocusKey | null;
    src?: string;
    selected?: boolean;
    multiSelect?: boolean;
    disabled?: boolean;
    tooltip?: ReactNode | null;
    selectSound?: unknown;
    uiTag?: string;
    className?: string;
    onSelect?: (value: unknown) => unknown;
} & HTMLAttributes<HTMLElement>;

type SectionProps = {
    title?: string | null;
    uiTag?: string;
    children?: ReactNode;
};

export type ToolOptionsComponents = {
    Section: (props: SectionProps) => JSX.Element;
    ToolButton: (props: ToolButtonProps) => JSX.Element;
    toolButtonClassName?: string;
};

type ExtendableComponentResult = {
    props?: {
        children?: ReactNode;
    };
} & ReactElement;

function updateInvertMode(nextMode: number) {
    TOGGLE_INVERTED(nextMode);
}

function withAppendedToolSection(result: ExtendableComponentResult, section: JSX.Element) {
    if (!isValidElement(result)) {
        return result;
    }

    const nextChildren = [...Children.toArray(result.props?.children), section];
    return cloneElement(result, {
        ...result.props,
        children: nextChildren,
    });
}

function InvertToggleSection({
    components,
    isInverted,
    onToggle,
}: {
    components: ToolOptionsComponents;
    isInverted: boolean;
    onToggle: () => void;
}) {
    const { Section, ToolButton, toolButtonClassName } = components;

    return (
        <Section title={SECTION_TITLE}>
            <ToolButton
                selected={isInverted}
                onSelect={onToggle}
                src={buttonImg}
                tooltip={TOOLTIP_TEXT}
                focusKey={FOCUS_DISABLED}
                className={toolButtonClassName}
            />
        </Section>
    );
}

export function createInvertLHTTool(components: ToolOptionsComponents): ModuleRegistryExtend {
    return (Component: any) => {
        return (props) => {
            const result = Component(props) as ExtendableComponentResult;
            const isShowing = useValue(SHOW_BINDING);
            const invertMode = useValue(INVERT_MODE_BINDING);
            const isInverted = invertMode === LEFT_HAND_TRAFFIC_MODE;

            if (!isShowing) {
                return result;
            }

            return withAppendedToolSection(
                result,
                <InvertToggleSection
                    components={components}
                    isInverted={isInverted}
                    onToggle={() => updateInvertMode(isInverted ? DEFAULT_MODE : LEFT_HAND_TRAFFIC_MODE)}
                />
            );
        };
    };
}
