import { BalloonDirection, Color, FocusKey, Theme, UniqueFocusKey } from "cs2/bindings";
import { InputAction } from "cs2/input";
import { ModuleRegistry } from "cs2/modding";
import { HTMLAttributes, ReactNode } from "react";

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
    children?: ReactNode;
    onSelect?: (value: unknown) => unknown;
} & HTMLAttributes<HTMLElement>;

type SectionProps = {
    title?: string | null;
    uiTag?: string;
    children?: ReactNode;
};

type ColorFieldProps = {
    focusKey?: FocusKey;
    disabled?: boolean;
    value?: Color;
    className?: string;
    selectAction?: InputAction;
    alpha?: unknown;
    popupDirection?: BalloonDirection;
    onChange?: (value: Color) => void;
    onClick?: (event: unknown) => void;
    onMouseEnter?: (event: unknown) => void;
    onMouseLeave?: (event: unknown) => void;
};

const registryIndex = {
    Section: ["game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", "Section"],
    ToolButton: ["game-ui/game/components/tool-options/tool-button/tool-button.tsx", "ToolButton"],
    toolButtonTheme: ["game-ui/game/components/tool-options/tool-button/tool-button.module.scss", "classes"],
    mouseToolOptionsTheme: ["game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.module.scss", "classes"],
    FOCUS_DISABLED: ["game-ui/common/focus/focus-key.ts", "FOCUS_DISABLED"],
    FOCUS_AUTO: ["game-ui/common/focus/focus-key.ts", "FOCUS_AUTO"],
    useUniqueFocusKey: ["game-ui/common/focus/focus-key.ts", "useUniqueFocusKey"],
    assetGridTheme: ["game-ui/game/components/asset-menu/asset-grid/asset-grid.module.scss", "classes"],
    descriptionTooltipTheme: ["game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss", "classes"],
    ColorField: ["game-ui/common/input/color-picker/color-field/color-field.tsx", "ColorField"],
} as const;

type RegistryEntry = keyof typeof registryIndex;

export class VanillaComponentResolver {
    public static get instance(): VanillaComponentResolver {
        return this._instance!;
    }

    private static _instance?: VanillaComponentResolver;

    public static setRegistry(registry: ModuleRegistry) {
        this._instance = new VanillaComponentResolver(registry);
    }

    private readonly registryData: ModuleRegistry;
    private readonly cachedData: Partial<Record<RegistryEntry, unknown>> = {};

    private constructor(registry: ModuleRegistry) {
        this.registryData = registry;
    }

    private updateCache(entry: RegistryEntry) {
        const [modulePath, exportName] = registryIndex[entry];
        const module = this.registryData.registry.get(modulePath);
        const resolvedValue = module?.[exportName];
        this.cachedData[entry] = resolvedValue;
        return resolvedValue;
    }

    private getCachedValue<T>(entry: RegistryEntry): T {
        return (this.cachedData[entry] ?? this.updateCache(entry)) as T;
    }

    public get Section(): (props: SectionProps) => JSX.Element {
        return this.getCachedValue("Section");
    }

    public get ToolButton(): (props: ToolButtonProps) => JSX.Element {
        return this.getCachedValue("ToolButton");
    }

    public get ColorField(): (props: ColorFieldProps) => JSX.Element {
        return this.getCachedValue("ColorField");
    }

    public get toolButtonTheme(): Theme & { ToolButton?: string } {
        return this.getCachedValue("toolButtonTheme");
    }

    public get mouseToolOptionsTheme(): Theme {
        return this.getCachedValue("mouseToolOptionsTheme");
    }

    public get assetGridTheme(): Theme {
        return this.getCachedValue("assetGridTheme");
    }

    public get descriptionTooltipTheme(): Theme {
        return this.getCachedValue("descriptionTooltipTheme");
    }

    public get FOCUS_DISABLED(): UniqueFocusKey {
        return this.getCachedValue("FOCUS_DISABLED");
    }

    public get FOCUS_AUTO(): UniqueFocusKey {
        return this.getCachedValue("FOCUS_AUTO");
    }

    public get useUniqueFocusKey(): (focusKey: FocusKey, debugName: string) => UniqueFocusKey | null {
        return this.getCachedValue("useUniqueFocusKey");
    }
}
