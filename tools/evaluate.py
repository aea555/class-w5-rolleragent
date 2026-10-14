"""Modeli ve rastgele ajanı N bölüm çalıştırıp başarı, düşme ve süre dolması oranlarını yazar.

Kullanım: python tools/evaluate.py <model.onnx> <bölüm sayısı>, ardından Unity'de Play.
"""
import sys

import numpy as np
import onnxruntime as ort
from mlagents_envs.base_env import ActionTuple
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

model_path, episodes = sys.argv[1], int(sys.argv[2])
session = ort.InferenceSession(model_path)
channel = EngineConfigurationChannel()
env = UnityEnvironment(file_name=None, side_channels=[channel])
channel.set_configuration_parameters(time_scale=20)
env.reset()
behavior = list(env.behavior_specs)[0]
rng = np.random.default_rng(0)

for policy in ("model", "random"):
    counts = {"success": 0, "fall": 0, "timeout": 0}
    lengths, steps = [], 0
    while sum(counts.values()) < episodes:
        decision, terminal = env.get_steps(behavior)
        for agent_id in terminal.agent_id:
            step = terminal[agent_id]
            if step.interrupted:
                counts["timeout"] += 1
            elif step.reward > 0.5:
                counts["success"] += 1
            else:
                counts["fall"] += 1
            lengths.append(steps)
            steps = 0
        if len(decision) > 0:
            obs = decision.obs[0].astype(np.float32)
            if policy == "model":
                action = session.run(["deterministic_continuous_actions"], {"obs_0": obs})[0]
            else:
                action = rng.uniform(-1, 1, (len(decision), 2)).astype(np.float32)
            env.set_actions(behavior, ActionTuple(continuous=np.clip(action, -1, 1)))
            steps += 1
        env.step()
    n = sum(counts.values())
    print(f"{policy}: başarı %{100 * counts['success'] / n:.1f}, düşme %{100 * counts['fall'] / n:.1f}, "
          f"süre doldu %{100 * counts['timeout'] / n:.1f}, ortalama uzunluk {np.mean(lengths):.0f} adım")

env.close()
