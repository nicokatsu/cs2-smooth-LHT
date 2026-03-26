import { bindValue, trigger, useValue } from "cs2/api";
import { ModuleRegistryExtend } from "cs2/modding";
import { Children, cloneElement, isValidElement, ReactElement, ReactNode } from "react";
import mod from "../../mod.json";
import buttonImg from "../../imgs/button.svg";
import styles from "./InvertLHTTool.module.scss";
import { VanillaComponentResolver } from "./VanillaComponentResolver";

const SHOW_BINDING = bindValue<boolean>(mod.id, "IsShowing");
const INVERT_MODE_BINDING = bindValue<number>(mod.id, "IsInverted");

const LEFT_HAND_TRAFFIC_MODE = 1;
const DEFAULT_MODE = 0;
const SECTION_TITLE = "LHT Invert Building Networks";
const TOOLTIP_TEXT = "Changes apply to all existing instances of this building.";

type ExtendableComponentResult = {
    props?: {
        children?: ReactNode;
    };
} & ReactElement;

function updateInvertMode(nextMode: number) {
    trigger(mod.id, "ToggleInverted", nextMode);
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

function InvertToggleSection({ isInverted, onToggle }: { isInverted: boolean; onToggle: () => void }) {
    const vanilla = VanillaComponentResolver.instance;

    return (
        <vanilla.Section title={SECTION_TITLE}>
            <vanilla.ToolButton
                selected={isInverted}
                onSelect={onToggle}
                src={buttonImg}
                tooltip={TOOLTIP_TEXT}
                focusKey={vanilla.FOCUS_DISABLED}
                className={vanilla.toolButtonTheme.ToolButton}
            >
                <span className={styles.centeredContentButton} />
            </vanilla.ToolButton>
        </vanilla.Section>
    );
}

export const InvertLHTTool: ModuleRegistryExtend = (Component: any) => {
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
                isInverted={isInverted}
                onToggle={() => updateInvertMode(isInverted ? DEFAULT_MODE : LEFT_HAND_TRAFFIC_MODE)}
            />
        );
    };
};
